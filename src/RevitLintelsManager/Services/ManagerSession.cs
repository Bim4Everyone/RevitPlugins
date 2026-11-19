using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

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

    public event Action Released;

    /// <inheritdoc />
    public void Start(IKernel kernel) {
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
            repository.UIApplication.ApplicationClosing += OnApplicationClosing;
            window.Closed += OnWindowClosed;
            window.Closing += OnWindowClosing;
            viewModel.CloseRequested += RequestClose;
            UpdateDocumentState();
            window.Show();
            if(_windowClosed || viewModel.State.IsClosing) {
                throw new OperationCanceledException();
            }
            _kernel = kernel;
            requests.StartAcceptingRequests();
        } catch {
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
        if(window.WindowState == WindowState.Minimized) {
            window.WindowState = WindowState.Normal;
        }
        window.Activate();
    }

    /// <inheritdoc />
    public void RequestClose() {
        if(!viewModel.State.IsClosing) {
            viewModel.State.IsClosing = true;
            viewModel.State.AdvanceGeneration();
        }
        requests.BeginClose();
        viewModel.SetBlocked(true);
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
            viewModel.State.AdvanceGeneration();
            requests.InvalidateRequests();
        }
        viewModel.SetBlocked(isBlocked);
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
