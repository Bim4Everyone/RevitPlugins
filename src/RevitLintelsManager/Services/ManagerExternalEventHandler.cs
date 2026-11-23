using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Autodesk.Revit.UI;

using dosymep.SimpleServices;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.Services;

/// <summary>Сериализует чтение контекста и владеет native ExternalEvent до безопасного закрытия.</summary>
internal sealed class ManagerExternalEventHandler : IManagerRequestDispatcher, IExternalEventHandler {
    private readonly object _sync = new();
    private readonly Queue<PendingRequest> _queue = new();
    private readonly RevitRepository _repository;
    private readonly ManagerSessionState _state;
    private readonly ILoggerService _logger;
    private readonly ILocalizationService _localization;
    private readonly ExternalEvent _externalEvent;
    private PendingRequest _activeRequest;
    private bool _accepting;
    private bool _closing;
    private bool _executing;
    private bool _signalPending;
    private bool _disposed;

    /// <summary>Создаёт ExternalEvent в исходном API-контексте команды.</summary>
    public ManagerExternalEventHandler(RevitRepository repository, ManagerSessionState state,
        ILoggerService logger, ILocalizationService localization) {
        _repository = repository;
        _state = state;
        _logger = logger;
        _localization = localization;
        _externalEvent = ExternalEvent.Create(this);
    }

    /// <inheritdoc />
    public void StartAcceptingRequests() {
        lock(_sync) {
            _accepting = !_closing && !_disposed;
        }
    }

    /// <inheritdoc />
    public Task<ManagerDocumentContext> Enqueue(ManagerRequest request) {
        if(request is null) {
            throw new ArgumentNullException(nameof(request));
        }
        var pending = new PendingRequest(request);
        lock(_sync) {
            if(!_accepting || !IsCurrent(request)) {
                pending.Completion.TrySetCanceled();
                return pending.Completion.Task;
            }
            _queue.Enqueue(pending);
            if(!_executing && !_signalPending) {
                Signal();
            }
        }
        return pending.Completion.Task;
    }

    /// <inheritdoc />
    public void InvalidateRequests() {
        lock(_sync) {
            CancelResults();
        }
    }

    /// <inheritdoc />
    public void BeginClose() {
        lock(_sync) {
            _closing = true;
            _accepting = false;
            CancelResults();
        }
    }

    /// <inheritdoc />
    public void PumpInApiContext() {
        lock(_sync) {
            if(_disposed || _executing) {
                return;
            }
            // Pending может относиться к уже завершающемуся Execute, а не к будущему вызову.
            _signalPending = _externalEvent.IsPending;
            if(!_closing && !_signalPending && _queue.Count > 0) {
                Signal();
            }
        }
    }

    /// <inheritdoc />
    public bool TryDisposeInApiContext() {
        BeginClose();
        lock(_sync) {
            if(_disposed) {
                return true;
            }
            if(_executing || _externalEvent.IsPending) {
                return false;
            }
            _externalEvent.Dispose();
            _disposed = true;
            return true;
        }
    }

    /// <summary>Страхует ошибку построения DI до запуска; живую сессию сначала закрывает её владелец.</summary>
    public void Dispose() {
        if(!TryDisposeInApiContext()) {
            throw new InvalidOperationException(_localization.GetLocalizedString("Manager.ClosePending"));
        }
    }

    /// <inheritdoc />
    public void Execute(UIApplication app) {
        int count;
        lock(_sync) {
            if(_disposed || _executing) {
                return;
            }
            _executing = true;
            _signalPending = false;
            count = _queue.Count;
        }
        try {
            // Новые запросы оставляем следующему callback, чтобы непрерывная очередь не удерживала Revit.
            for(int index = 0; index < count; index++) {
                PendingRequest pending;
                lock(_sync) {
                    if(_queue.Count == 0) {
                        break;
                    }
                    pending = _queue.Dequeue();
                    if(!IsCurrent(pending.Request)) {
                        pending.Completion.TrySetCanceled();
                        continue;
                    }
                    _activeRequest = pending;
                }
                ReadContext(app, pending);
            }
        } finally {
            lock(_sync) {
                _activeRequest = null;
                _executing = false;
                // Raise на границе возврата может дать Pending текущего callback; повторяет только Idling.
            }
        }
    }

    /// <inheritdoc />
    public string GetName() {
        return nameof(RevitLintelsManager) + ".ReadDocumentContext";
    }

    // Проверяет только независимое состояние; вызывается под блокировкой очереди.
    private bool IsCurrent(ManagerRequest request) {
        return !_closing && !_disposed && !_state.IsClosing && !_state.IsBlocked
            && request.DocumentToken == _state.DocumentToken && request.Generation == _state.Generation;
    }

    // Сигнализирует под блокировкой; отказ завершает все ожидающие задачи без повторного цикла Raise.
    private void Signal() {
        try {
            var result = _externalEvent.Raise();
            _signalPending = result is ExternalEventRequest.Accepted or ExternalEventRequest.Pending;
            if(!_signalPending) {
                FailResults(new InvalidOperationException(_localization.GetLocalizedString("Manager.SignalRejected")));
                _logger.Warning(new InvalidOperationException(result.ToString()), "Revit отклонил запрос менеджера.");
            }
        } catch(Exception exception) {
            _signalPending = false;
            FailResults(new InvalidOperationException(
                _localization.GetLocalizedString("Manager.SignalRejected"), exception));
            _logger.Warning(exception, "Ошибка передачи запроса менеджера в Revit.");
        }
    }

    // Читает модель только внутри Execute и повторно проверяет поколение перед публикацией результата.
    private void ReadContext(UIApplication app, PendingRequest pending) {
        try {
            var context = _repository.ReadDocumentContext(app, pending.Request, _state.Config);
            lock(_sync) {
                if(context is null || !IsCurrent(pending.Request)) {
                    pending.Completion.TrySetCanceled();
                } else {
                    pending.Completion.TrySetResult(context);
                }
            }
        } catch(Exception exception) {
            lock(_sync) {
                if(!IsCurrent(pending.Request)) {
                    pending.Completion.TrySetCanceled();
                } else {
                    pending.Completion.TrySetException(new InvalidOperationException(
                        _localization.GetLocalizedString("Manager.ReadFailed"), exception));
                }
            }
            _logger.Warning(exception, "Ошибка чтения контекста менеджера перемычек.");
        } finally {
            lock(_sync) {
                _activeRequest = null;
            }
        }
    }

    // Отменяет результаты, но не уничтожает ожидающий native callback.
    private void CancelResults() {
        _activeRequest?.Completion.TrySetCanceled();
        while(_queue.Count > 0) {
            _queue.Dequeue().Completion.TrySetCanceled();
        }
    }

    // Завершает очередь исключением, если native-сигнал не был принят.
    private void FailResults(Exception exception) {
        while(_queue.Count > 0) {
            _queue.Dequeue().Completion.TrySetException(exception);
        }
    }

    /// <summary>Запрос и однократно завершаемый результат.</summary>
    private sealed class PendingRequest(ManagerRequest request) {
        public ManagerRequest Request { get; } = request;
        public TaskCompletionSource<ManagerDocumentContext> Completion { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
