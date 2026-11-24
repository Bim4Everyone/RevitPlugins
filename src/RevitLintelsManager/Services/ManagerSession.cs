using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

using dosymep.Revit;
using dosymep.SimpleServices;

using Ninject;

using RevitLintelsManager.Models;
using RevitLintelsManager.ViewModels;
using RevitLintelsManager.Views;

namespace RevitLintelsManager.Services;

/// <summary>Владелец окна и контейнера; API доступен только команде и callbacks Revit.</summary>
internal sealed class ManagerSession(RevitRepository repository, ManagerWindow window,
    ManagerViewModel viewModel, WindowPlatformEvents platformEvents, ILoggerService logger,
    IManagerRequestDispatcher requests) : IManagerSession {
    private IKernel _kernel;
    private bool _subscribed;
    private bool _windowClosed;
    private bool _documentClosing;
    private bool _releaseScheduled;
    private bool _started;
    private bool _refreshAfterReturn;
    private long? _observedGeneration;
    private ManagerScopeStamp _observedScope;

    public event Action Released;

    /// <inheritdoc />
    public void Start(IKernel kernel, Action<Exception> reportStartupFailure) {
        if(_started) {
            throw new InvalidOperationException();
        }
        _started = true;
        AppDomain.CurrentDomain.AssemblyResolve += ResolveSessionAssembly;
        try {
            _subscribed = true;
            repository.UIApplication.Idling += OnIdling;
            repository.UIApplication.ViewActivated += OnViewActivated;
            repository.Application.DocumentClosing += OnDocumentClosing;
            repository.Application.DocumentClosed += OnDocumentClosed;
            repository.Application.DocumentChanged += OnDocumentChanged;
            repository.UIApplication.ApplicationClosing += OnApplicationClosing;
            window.Closed += OnWindowClosed;
            window.Closing += OnWindowClosing;
            viewModel.CloseRequested += RequestClose;
            UpdateDocumentState();
            requests.StartAcceptingRequests();
            _kernel = kernel;
            window.Dispatcher.InvokeAsync(() => ShowWindowAsync(reportStartupFailure));
        } catch {
            _kernel = null;
            requests.BeginClose();
            requests.TryDisposeInApiContext();
            Unsubscribe();
            platformEvents.Dispose();
            try {
                if(!_windowClosed) {
                    window.Close();
                }
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= ResolveSessionAssembly;
            }
            throw;
        }
    }

    /// <inheritdoc />
    public void Activate() {
        if(viewModel.State.IsClosing) {
            FinishInApiContext();
            return;
        }
        UpdateDocumentState();
        if(!window.IsVisible) {
            return;
        }
        if(window.WindowState == WindowState.Minimized) {
            window.WindowState = WindowState.Normal;
        }
        window.Activate();
    }

    /// <inheritdoc />
    public void RequestClose() {
        if(!viewModel.State.IsClosing) {
            viewModel.State.IsClosing = true;
            viewModel.State.NavigationStamp = null;
            viewModel.State.AdvanceGeneration();
        }
        requests.BeginClose();
        viewModel.SetBlocked(true);
    }

    // Показывает окно после завершения первого запроса, если исходная сессия ещё открыта.
    private async Task ShowWindowAsync(Action<Exception> reportStartupFailure) {
        try {
            if(viewModel.State.IsClosing) {
                return;
            }
            await viewModel.LoadInitialContextAsync();
            if(!viewModel.State.IsClosing) {
                window.Show();
            }
        } catch(Exception exception) {
            try {
                reportStartupFailure(exception);
            } finally {
                RequestClose();
            }
        }
    }

    // Оставляет окно живым до безопасного возврата pending/active обработчика.
    private void OnWindowClosing(object sender, CancelEventArgs e) {
        e.Cancel = true;
        RequestClose();
    }

    // Запоминает закрытие без обращения к Revit API из WPF.
    private void OnWindowClosed(object sender, EventArgs e) {
        _windowClosed = true;
        RequestClose();
    }

    // Проверяет документ и завершает отложенное освобождение в разрешённом контексте.
    private void OnIdling(object sender, IdlingEventArgs e) {
        if(viewModel.State.IsClosing || !repository.Document.IsValidObject) {
            FinishInApiContext();
            return;
        }
        _documentClosing = false;
        UpdateDocumentState();
        requests.PumpInApiContext();
        RefreshChangedScope();
    }

    // Блокирует окно сразу после смены активного документа.
    private void OnViewActivated(object sender, ViewActivatedEventArgs e) {
        UpdateDocumentState();
    }

    // Блокирует сессию до подтверждения закрытия исходного документа.
    private void OnDocumentClosing(object sender, DocumentClosingEventArgs e) {
        if(repository.Document.IsValidObject && repository.Document.Equals(e.Document)) {
            _documentClosing = true;
            SetDocumentBlocked(true);
        }
    }

    // Не читает свойства уже закрытого документа; отменённое закрытие сверяется на Idling.
    private void OnDocumentClosed(object sender, DocumentClosedEventArgs e) {
        if(!repository.Document.IsValidObject) {
            FinishInApiContext();
        }
    }

    // Инвалидирует снимок исходного документа без автоматического чтения после правки или Undo.
    private void OnDocumentChanged(object sender, DocumentChangedEventArgs e) {
        if(viewModel.State.IsClosing || !repository.Document.IsValidObject
            || !repository.Document.Equals(e.GetDocument())) {
            return;
        }
        if(requests.IsPreparingManagerView) {
            var transactions = e.GetTransactionNames();
            if(transactions.SequenceEqual(new[] { RevitRepository.ManagerViewTransactionName })
                || transactions.SequenceEqual(new[] { repository.ManagerSectionBoxTransactionName })) {
                return;
            }
        }
        if(viewModel.IsRefreshing && viewModel.LastContext?.ScopeStamp is { } acceptedScope
            && _observedScope?.Scope == acceptedScope.Scope) {
            // Позволяет Idling снова заметить смену источника, если её запрос ещё не применён.
            _observedScope = acceptedScope;
        }
        viewModel.State.AdvanceGeneration();
        requests.InvalidateRequests();
        viewModel.InvalidateModel(e.GetDeletedElementIds().Select(id => (long) id.GetIdValue()));
    }

    // Завершает сессию при выходе из приложения.
    private void OnApplicationClosing(object sender, ApplicationClosingEventArgs e) {
        FinishInApiContext();
    }

    // Передаёт в GUI только результат проверки идентичности исходного документа.
    private void UpdateDocumentState() {
        var activeDocument = repository.UIApplication.ActiveUIDocument?.Document;
        bool isSourceDocument = repository.Document.IsValidObject
            && activeDocument?.IsValidObject == true
            && repository.Document.Equals(activeDocument);
        SetDocumentBlocked(_documentClosing || !isSourceDocument);
    }

    // Смена доступности документа инвалидирует задачи и ещё не опубликованные UI-ответы.
    private void SetDocumentBlocked(bool isBlocked) {
        if(viewModel.State.IsBlocked != isBlocked) {
            viewModel.State.NavigationStamp = null;
            viewModel.State.AdvanceGeneration();
            requests.InvalidateRequests();
            _refreshAfterReturn = !isBlocked;
        }
        viewModel.SetBlocked(isBlocked);
    }

    // Сравнивает источник текущего диапазона на Idling и ставит изменившийся диапазон в существующую очередь.
    private void RefreshChangedScope() {
        if(!window.IsVisible || !viewModel.RefreshCommand.CanExecute(null)) {
            return;
        }
        var context = viewModel.LastContext;
        if(context is not null && _observedGeneration != context.Generation) {
            _observedGeneration = context.Generation;
            _observedScope = context.ScopeStamp;
        }
        var scope = viewModel.SelectedScope?.Scope ?? viewModel.State.InitialScope;
        var navigation = viewModel.State.NavigationStamp;
        viewModel.State.NavigationStamp = null;
        if(!_refreshAfterReturn && (_observedScope is null || scope == ManagerScope.Project)) {
            return;
        }

        var currentScope = repository.CaptureScopeStamp(scope);
        bool sourceChanged = _observedScope?.Scope == scope && !_observedScope.Matches(currentScope);
        bool ownNavigation = navigation?.Matches(viewModel.State, currentScope) == true;
        _observedScope = currentScope;
        if(_refreshAfterReturn || (sourceChanged && !ownNavigation)) {
            _refreshAfterReturn = false;
            viewModel.RefreshCommand.Execute(null);
        }
    }

    // Снимает API-подписки до выхода из callback, а контейнер освобождает после его возврата.
    private void FinishInApiContext() {
        if(_releaseScheduled) {
            return;
        }
        RequestClose();
        if(!requests.TryDisposeInApiContext()) {
            return;
        }
        _releaseScheduled = true;
        Unsubscribe();
        window.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(ReleaseResources));
    }

    // Удаляет подписки, созданные в исходном API-контексте.
    private void Unsubscribe() {
        if(!_subscribed) {
            return;
        }
        _subscribed = false;
        repository.UIApplication.Idling -= OnIdling;
        repository.UIApplication.ViewActivated -= OnViewActivated;
        repository.Application.DocumentClosing -= OnDocumentClosing;
        repository.Application.DocumentClosed -= OnDocumentClosed;
        repository.Application.DocumentChanged -= OnDocumentChanged;
        repository.UIApplication.ApplicationClosing -= OnApplicationClosing;
        window.Closed -= OnWindowClosed;
        window.Closing -= OnWindowClosing;
        viewModel.CloseRequested -= RequestClose;
    }

    // Освобождает только WPF и управляемые ресурсы, без чтения Document.
    private void ReleaseResources() {
        try {
            if(!_windowClosed) {
                window.Close();
                _windowClosed = true;
            }
        } catch(Exception exception) {
            logger.Warning(exception, "Ошибка закрытия менеджера перемычек.");
        } finally {
            platformEvents.Dispose();
            window.DataContext = null;
            try {
                _kernel?.Dispose();
            } catch(Exception exception) {
                logger.Warning(exception, "Ошибка освобождения контейнера менеджера перемычек.");
            } finally {
                _kernel = null;
                AppDomain.CurrentDomain.AssemblyResolve -= ResolveSessionAssembly;
                Released?.Invoke();
                Released = null;
            }
        }
    }

    // Продлевает существующий корпоративный механизм разрешения DLL до завершения сессии.
    private Assembly ResolveSessionAssembly(object sender, ResolveEventArgs args) {
        return dosymep.AppDomainExtensions.CurrentDomain_AssemblyResolve(sender, args);
    }
}
