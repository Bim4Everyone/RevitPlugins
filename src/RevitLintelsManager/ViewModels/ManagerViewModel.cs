using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitLintelsManager.Models;
using RevitLintelsManager.Services;

namespace RevitLintelsManager.ViewModels;

internal sealed class ManagerViewModel : BaseViewModel {
    public event Action CloseRequested;
    private readonly IManagerRequestDispatcher _requests;
    private readonly Dispatcher _dispatcher;
    private readonly ILocalizationService _localization;
    private readonly ObservableCollection<ManagerRowViewModel> _visibleRows = new();
    private bool _isBlocked;
    private bool _isRefreshing;
    private string _contextSummary;
    private ManagerDocumentContext _lastContext;
    private IReadOnlyList<ManagerGroupViewModel> _groups = Array.Empty<ManagerGroupViewModel>();

    /// <summary>Создаёт команды окна без непосредственных зависимостей от Revit API.</summary>
    public ManagerViewModel(ManagerSessionState state, ISettingsCommandFactory commands,
        IManagerRequestDispatcher requests, Dispatcher dispatcher, ILocalizationService localization) {
        State = state;
        _requests = requests;
        _dispatcher = dispatcher;
        _localization = localization;
        VisibleRows = new ReadOnlyObservableCollection<ManagerRowViewModel>(_visibleRows);
        ContextSummary = localization.GetLocalizedString("Manager.Refresh");
        RefreshCommand = commands.CreateAsync(RefreshContextAsync,
            () => !IsBlocked && !State.IsClosing && !IsRefreshing);
        CloseCommand = commands.Create(() => CloseRequested?.Invoke());
    }

    public ManagerSessionState State { get; }
    public ReadOnlyObservableCollection<ManagerRowViewModel> VisibleRows { get; }
    public ICommand CloseCommand { get; }
    public ICommand RefreshCommand { get; }
    public ManagerDocumentContext LastContext {
        get => _lastContext;
        private set => RaiseAndSetIfChanged(ref _lastContext, value);
    }
    public string ContextSummary {
        get => _contextSummary;
        private set => RaiseAndSetIfChanged(ref _contextSummary, value);
    }
    public bool IsRefreshing {
        get => _isRefreshing;
        private set => RaiseAndSetIfChanged(ref _isRefreshing, value);
    }
    public bool IsBlocked {
        get => _isBlocked;
        private set => RaiseAndSetIfChanged(ref _isBlocked, value);
    }

    /// <summary>Заменяет готовую проекцию групп без получения данных из Revit.</summary>
    /// <param name="groups">Материализованные заголовки с дочерними строками.</param>
    public void SetGroups(IReadOnlyList<ManagerGroupViewModel> groups) {
        _groups = (groups ?? throw new ArgumentNullException(nameof(groups))).ToList().AsReadOnly();
        RebuildVisibleRows();
    }

    /// <summary>Обновляет плоскую проекцию; UI-команда раскрытия подключается в TASK-005.</summary>
    /// <param name="group">Группа текущей проекции.</param>
    /// <param name="isExpanded">Требуемая видимость дочерних строк.</param>
    public void SetExpanded(ManagerGroupViewModel group, bool isExpanded) {
        if(!_groups.Contains(group)) {
            return;
        }
        group.IsExpanded = isExpanded;
        RebuildVisibleRows();
    }

    /// <summary>Применяет проверенное в API-контексте состояние исходного документа.</summary>
    public void SetBlocked(bool isBlocked) {
        State.IsBlocked = isBlocked;
        IsBlocked = isBlocked;
        CommandManager.InvalidateRequerySuggested();
    }

    // В коллекцию попадают только заголовки и дети раскрытых групп, без визуальных контейнеров.
    private void RebuildVisibleRows() {
        _visibleRows.Clear();
        foreach(var group in _groups) {
            _visibleRows.Add(group);
            if(group.IsExpanded) {
                foreach(var item in group.Items) {
                    _visibleRows.Add(item);
                }
            }
        }
    }

    // Передаёт только token/generation в очередь и публикует независимый ответ на UI Dispatcher.
    private async Task RefreshContextAsync() {
        IsRefreshing = true;
        CommandManager.InvalidateRequerySuggested();
        var request = new ManagerRequest(State.DocumentToken, State.AdvanceGeneration());
        try {
            var context = await _requests.Enqueue(request).ConfigureAwait(false);
            if(!_dispatcher.HasShutdownStarted) {
                await _dispatcher.InvokeAsync(() => ApplyContext(context)).Task.ConfigureAwait(false);
            }
        } catch(InvalidOperationException exception) {
            if(!_dispatcher.HasShutdownStarted) {
                await _dispatcher.InvokeAsync(() => {
                    if(IsCurrentResponse(request.DocumentToken, request.Generation)) {
                        ContextSummary = exception.Message;
                    }
                }).Task.ConfigureAwait(false);
            }
        } finally {
            if(!_dispatcher.HasShutdownStarted) {
                await _dispatcher.InvokeAsync(() => {
                    IsRefreshing = false;
                    CommandManager.InvalidateRequerySuggested();
                }).Task.ConfigureAwait(false);
            }
        }
    }

    // Отбрасывает результат, устаревший между завершением API и исполнением UI continuation.
    private void ApplyContext(ManagerDocumentContext context) {
        if(!IsCurrentResponse(context.DocumentToken, context.Generation)) {
            return;
        }
        LastContext = context;
        ContextSummary = string.Format(CultureInfo.CurrentCulture,
            _localization.GetLocalizedString("Manager.ContextSummary"), context.DocumentTitle,
            context.ViewName, context.SelectionUniqueIds.Count);
    }

    // Проверяет актуальность успешного ответа и ошибки в момент их применения на UI-потоке.
    private bool IsCurrentResponse(Guid documentToken, long generation) {
        return !State.IsClosing && !State.IsBlocked && documentToken == State.DocumentToken
            && generation == State.Generation;
    }
}
