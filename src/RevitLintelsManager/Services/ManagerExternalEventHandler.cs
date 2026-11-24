using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Autodesk.Revit.UI;

using dosymep.SimpleServices;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.Services;

/// <summary>Сериализует чтение и навигацию, владеет ExternalEvent до закрытия.</summary>
internal sealed class ManagerExternalEventHandler : IManagerRequestDispatcher, IExternalEventHandler {
    private readonly object _sync = new();
    private readonly Queue<PendingRequest> _queue = new();
    private readonly RevitRepository _repository;
    private readonly ManagerSessionState _state;
    private readonly IManagerNavigationService _navigation;
    private readonly ILoggerService _logger;
    private readonly ILocalizationService _localization;
    private readonly ExternalEvent _externalEvent;
    private PendingRequest _activeRequest;
    private bool _accepting;
    private bool _closing;
    private bool _executing;
    private bool _signalPending;
    private bool _disposed;
    private bool _managerViewPrepared;

    /// <summary>Создаёт ExternalEvent в исходном API-контексте команды.</summary>
    public ManagerExternalEventHandler(RevitRepository repository, ManagerSessionState state,
        IManagerNavigationService navigation, ILoggerService logger, ILocalizationService localization) {
        _repository = repository;
        _state = state;
        _navigation = navigation;
        _logger = logger;
        _localization = localization;
        _externalEvent = ExternalEvent.Create(this);
    }

    /// <inheritdoc />
    public bool IsPreparingManagerView { get; private set; }

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
            while(_queue.Count > 0 && !IsCurrent(_queue.Peek().Request)) {
                _queue.Dequeue().Completion.TrySetCanceled();
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
                ExecuteRequest(app, pending);
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
        return nameof(RevitLintelsManager) + ".ManagerRequests";
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
                _logger.Warning(new InvalidOperationException(result.ToString()),
                    _localization.GetLocalizedString("Manager.SignalRejected"));
            }
        } catch(Exception exception) {
            _signalPending = false;
            FailResults(new InvalidOperationException(
                _localization.GetLocalizedString("Manager.SignalRejected"), exception));
            _logger.Warning(exception, _localization.GetLocalizedString("Manager.SignalRejected"));
        }
    }

    // Выполняет один запрос и проверяет его поколение перед публикацией результата.
    private void ExecuteRequest(UIApplication app, PendingRequest pending) {
        bool isNavigation = pending.Request.NavigationItems is not null;
        try {
            if(!_repository.IsSourceDocumentActive(app)) {
                pending.Completion.TrySetCanceled();
                return;
            }
            ManagerDocumentContext context = null;
            bool completed = isNavigation
                ? NavigateInApiContext(app, pending.Request)
                : (context = ReadInApiContext(app, pending.Request)) is not null;
            lock(_sync) {
                if(!completed || !IsCurrent(pending.Request)) {
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
                    pending.Completion.TrySetException(!isNavigation && _managerViewPrepared
                        ? new InvalidOperationException(_localization.GetLocalizedString("Manager.ReadFailed"),
                            exception) : exception);
                }
            }
            string errorKey = isNavigation ? "Manager.NavigationFailed"
                : _managerViewPrepared ? "Manager.ReadFailed" : "Manager.ViewPreparationFailed";
            _logger.Warning(exception, _localization.GetLocalizedString(errorKey));
        } finally {
            lock(_sync) {
                _activeRequest = null;
            }
        }
    }

    // Повторно проверяет запрос после подготовки вида, которая может вызвать события документа.
    private ManagerDocumentContext ReadInApiContext(UIApplication app, ManagerRequest request) {
        if(!PrepareManagerView(app)) {
            return null;
        }
        lock(_sync) {
            if(!IsCurrent(request)) {
                return null;
            }
        }
        return _repository.ReadDocumentContext(app, request, _state);
    }

    // Отмечает собственные транзакции вида и преобразует ожидаемый отказ в локализованную диагностику.
    private bool NavigateInApiContext(UIApplication app, ManagerRequest request) {
        ManagerNavigationResult result;
        IsPreparingManagerView = true;
        try {
            result = _navigation.Navigate(app, request, _state);
        } catch(Exception exception) {
            throw new InvalidOperationException(_localization.GetLocalizedString("Manager.NavigationFailed"),
                exception);
        } finally {
            IsPreparingManagerView = false;
        }

        return result switch {
            ManagerNavigationResult.Completed => true,
            ManagerNavigationResult.SourceUnavailable => false,
            ManagerNavigationResult.TargetsUnavailable => throw new InvalidOperationException(
                _localization.GetLocalizedString("Manager.NavigationTargetsUnavailable")),
            _ => throw new InvalidOperationException(
                _localization.GetLocalizedString("Manager.NavigationBoundsUnavailable"))
        };
    }

    // Повторяет неудачную подготовку при обновлении, успешную выполняет один раз за запуск.
    private bool PrepareManagerView(UIApplication app) {
        if(_managerViewPrepared) {
            return true;
        }
        IsPreparingManagerView = true;
        try {
            _managerViewPrepared = _navigation.EnsureView(app, _state.Config) is not null;
            return _managerViewPrepared;
        } finally {
            IsPreparingManagerView = false;
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
