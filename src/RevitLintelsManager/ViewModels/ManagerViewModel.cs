using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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

internal sealed class ManagerViewModel : BaseViewModel, IDisposable {
    private sealed class VisibleRowCollection : ObservableCollection<ManagerRowViewModel> {
        /// <summary>Выполняет массовое обновление одним поддерживаемым WPF событием.</summary>
        public void ReplaceRows(IReadOnlyList<ManagerRowViewModel> rows) {
            CheckReentrancy();
            bool countChanged = Count != rows.Count;
            Items.Clear();
            foreach(var row in rows) {
                Items.Add(row);
            }
            if(countChanged) {
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            }
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    public event Action CloseRequested;
    public event Action<ManagerDetailsViewModel> DetailsRequested;
    public event Action DetailsDismissRequested;
    public event Action<ManagerScope> ProjectionChanging;
    public event Action<ManagerScope> ProjectionChanged;
    private const int _maximumIncrementalChanges = 128;
    private readonly IManagerRequestDispatcher _requests;
    private readonly Dispatcher _dispatcher;
    private readonly ILocalizationService _localization;
    private readonly VisibleRowCollection _visibleRows = new();
    private readonly HashSet<ManagerGroupKey> _expandedGroups = new();
    private Dictionary<ManagerItemKey, ManagerItemViewModel> _items = new();
    private HashSet<ManagerElementViewModel> _elements = new();
    private bool _isBlocked;
    private bool _isRefreshing;
    private string _contextSummary;
    private ManagerDocumentContext _lastContext;
    private int _processingCount;
    private ManagerScopeViewModel _selectedScope;
    private ManagerScope _projectionScope;
    private IReadOnlyList<ManagerGroupViewModel> _groups = Array.Empty<ManagerGroupViewModel>();

    /// <summary>Создаёт команды окна без непосредственных зависимостей от Revit API.</summary>
    public ManagerViewModel(ManagerSessionState state, ISettingsCommandFactory commands,
        IManagerRequestDispatcher requests, Dispatcher dispatcher, ILocalizationService localization) {
        State = state;
        _requests = requests;
        _dispatcher = dispatcher;
        _localization = localization;
        Scopes = Enum.GetValues(typeof(ManagerScope)).Cast<ManagerScope>()
            .Select(scope => new ManagerScopeViewModel(scope, localization)).ToList().AsReadOnly();
        _selectedScope = Scopes.First(scope => scope.Scope == state.InitialScope);
        _projectionScope = state.InitialScope;
        VisibleRows = new ReadOnlyObservableCollection<ManagerRowViewModel>(_visibleRows);
        ContextSummary = localization.GetLocalizedString("Manager.Refresh");
        RefreshCommand = commands.CreateAsync(RefreshContextAsync,
            () => !IsBlocked && !State.IsClosing && !IsRefreshing);
        CloseCommand = commands.Create(() => CloseRequested?.Invoke());
        SelectScopeCommand = commands.Create<ManagerScopeViewModel>(SelectScope, _ => CanChangeScope);
        ToggleGroupCommand = commands.Create<ManagerGroupViewModel>(
            group => SetExpanded(group, !group.IsExpanded),
            group => !IsBlocked && !State.IsClosing && !IsRefreshing && _groups.Contains(group));
        ShowInformationCommand = commands.Create<ManagerElementViewModel>(
            element => DetailsRequested?.Invoke(new ManagerDetailsViewModel(element, false, _localization)),
            element => CanEditRows && element.ShowInformation && ContainsElement(element));
        ShowWarningsCommand = commands.Create<ManagerElementViewModel>(
            element => DetailsRequested?.Invoke(new ManagerDetailsViewModel(element, true, _localization)),
            element => CanEditRows && ContainsElement(element));
    }

    public ManagerSessionState State { get; }
    public ReadOnlyObservableCollection<ManagerRowViewModel> VisibleRows { get; }
    public IReadOnlyList<ManagerGroupViewModel> Groups => _groups;
    public IReadOnlyList<ManagerScopeViewModel> Scopes { get; }
    public ManagerScopeViewModel SelectedScope {
        get => _selectedScope;
        set => RaiseAndSetIfChanged(ref _selectedScope, value);
    }
    public ICommand CloseCommand { get; }
    public ICommand SelectScopeCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ToggleGroupCommand { get; }
    public ICommand ShowInformationCommand { get; }
    public ICommand ShowWarningsCommand { get; }
    public ICommand ProjectScopeCommand { get; private set; }
    public ICommand ViewScopeCommand { get; private set; }
    public ICommand SelectedScopeCommand { get; private set; }
    public bool CanChangeScope => CanEditRows && ProjectScopeCommand is not null;
    public string ScopeToolTip => ProjectScopeCommand is null
        ? _localization.GetLocalizedString("Manager.NotImplemented") : ContextSummary;
    public int ProcessingCount => _processingCount;
    public string ProcessText => string.Format(CultureInfo.CurrentCulture,
        _localization.GetLocalizedString("Manager.ProcessCount"), ProcessingCount);
    public bool CanEditRows => !IsBlocked && !State.IsClosing && !IsRefreshing;
    public string GeneralMessage => State.IsClosing ? _localization.GetLocalizedString("Manager.Closing")
        : IsBlocked ? _localization.GetLocalizedString("Manager.DocumentChanged")
        : IsRefreshing ? _localization.GetLocalizedString("Manager.Busy") : null;
    public bool HasGeneralMessage => GeneralMessage is not null;
    public ManagerDocumentContext LastContext {
        get => _lastContext;
        private set => RaiseAndSetIfChanged(ref _lastContext, value);
    }
    public string ContextSummary {
        get => _contextSummary;
        private set {
            RaiseAndSetIfChanged(ref _contextSummary, value);
            RaisePropertyChanged(nameof(ScopeToolTip));
        }
    }
    public bool IsRefreshing {
        get => _isRefreshing;
        private set {
            RaiseAndSetIfChanged(ref _isRefreshing, value);
            RaisePropertyChanged(nameof(CanEditRows));
            NotifyPresentationState();
        }
    }
    public bool IsBlocked {
        get => _isBlocked;
        private set => RaiseAndSetIfChanged(ref _isBlocked, value);
    }

    /// <summary>Подключает команды диапазонов из композиции, когда источник поддерживает их.</summary>
    /// <param name="project">Команда всего набора.</param>
    /// <param name="view">Команда набора видимых элементов.</param>
    /// <param name="selected">Команда набора выделенных элементов.</param>
    public void SetScopeCommands(ICommand project, ICommand view, ICommand selected) {
        ProjectScopeCommand = project;
        ViewScopeCommand = view;
        SelectedScopeCommand = selected;
        RaisePropertyChanged(nameof(ProjectScopeCommand));
        RaisePropertyChanged(nameof(ViewScopeCommand));
        RaisePropertyChanged(nameof(SelectedScopeCommand));
        RaisePropertyChanged(nameof(CanChangeScope));
        RaisePropertyChanged(nameof(ScopeToolTip));
    }

    /// <summary>Сверяет полный набор строк и публикует группы для выбранного диапазона.</summary>
    /// <param name="items">Известные существующие строки, включая скрытые текущим диапазоном.</param>
    /// <param name="visibleKeys">Ключи строк выбранного диапазона в порядке источника.</param>
    /// <param name="scope">Диапазон публикуемой проекции.</param>
    public void SetItems(IReadOnlyList<ManagerItemViewModel> items, IEnumerable<ManagerItemKey> visibleKeys,
        ManagerScope scope) {
        var replacement = items.ToDictionary(item => item.Snapshot.Key);
        var visibleItems = visibleKeys.Distinct().Select(key => replacement[key]).ToList();
        foreach(var item in items) {
            if(_items.TryGetValue(item.Snapshot.Key, out var previous) && !ReferenceEquals(previous, item)) {
                item.RestoreState(previous);
            }
        }
        _items = replacement;
        _expandedGroups.IntersectWith(items.Select(item => item.Snapshot.GroupKey));

        var previousGroups = _groups.ToDictionary(group => group.Key);
        var groups = new List<ManagerGroupViewModel>();
        foreach(var group in visibleItems.GroupBy(item => item.Snapshot.GroupKey)) {
            var children = group.ToList();
            if(previousGroups.TryGetValue(group.Key, out var previous) && previous.Items.SequenceEqual(children)) {
                groups.Add(previous);
                continue;
            }
            for(int index = 0; index < children.Count; index++) {
                children[index].IsLastInGroup = index == children.Count - 1;
            }
            var first = children[0];
            var created = new ManagerGroupViewModel(group.Key, first.Opening.Element, first.Lintel.Element,
                children, _localization) { IsExpanded = _expandedGroups.Contains(group.Key) };
            created.StateChanged += OnGroupStateChanged;
            created.ExpansionChanged += OnGroupExpansionChanged;
            groups.Add(created);
        }
        if(scope == _projectionScope && _groups.SequenceEqual(groups)) {
            return;
        }

        ProjectionChanging?.Invoke(_projectionScope);
        DetailsDismissRequested?.Invoke();
        var retainedGroups = new HashSet<ManagerGroupViewModel>(groups);
        foreach(var group in _groups.Where(group => !retainedGroups.Contains(group))) {
            group.Dispose();
        }
        _projectionScope = scope;
        _groups = groups.AsReadOnly();
        _elements = new HashSet<ManagerElementViewModel>(_groups.SelectMany(group =>
            new[] { group.Opening, group.Lintel }.Concat(
                group.Items.SelectMany(item => new[] { item.Opening, item.Lintel }))));
        RaisePropertyChanged(nameof(Groups));
        SetProcessingCount(_groups.Sum(group => group.ProcessingCount));
        UpdateVisibleRows(CreateVisibleRows());
        ProjectionChanged?.Invoke(scope);
    }

    /// <summary>Обновляет только видимые строки выбранной группы, сохраняя дочерние VM.</summary>
    /// <param name="group">Группа текущей проекции.</param>
    /// <param name="isExpanded">Требуемая видимость дочерних строк.</param>
    public void SetExpanded(ManagerGroupViewModel group, bool isExpanded) {
        if(!_groups.Contains(group) || group.IsExpanded == isExpanded) {
            return;
        }
        group.IsExpanded = isExpanded;
    }

    /// <summary>Применяет проверенное в API-контексте состояние исходного документа.</summary>
    public void SetBlocked(bool isBlocked) {
        if(IsBlocked == isBlocked && !State.IsClosing) {
            return;
        }
        State.IsBlocked = isBlocked;
        IsBlocked = isBlocked;
        RaisePropertyChanged(nameof(CanEditRows));
        NotifyPresentationState();
        CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>Снимает подписки менеджера и групп при освобождении контейнера сессии.</summary>
    public void Dispose() {
        DetailsDismissRequested?.Invoke();
        foreach(var group in _groups) {
            group.StateChanged -= OnGroupStateChanged;
            group.ExpansionChanged -= OnGroupExpansionChanged;
            group.Dispose();
        }
        _groups = Array.Empty<ManagerGroupViewModel>();
        _items.Clear();
        _elements.Clear();
        _expandedGroups.Clear();
        _visibleRows.Clear();
        CloseRequested = null;
        DetailsRequested = null;
        DetailsDismissRequested = null;
        ProjectionChanging = null;
        ProjectionChanged = null;
    }

    /// <summary>Обновляет подписи существующих строк при смене языка платформы.</summary>
    public void RefreshLocalization() {
        DetailsDismissRequested?.Invoke();
        foreach(var scope in Scopes) {
            scope.RefreshLocalization();
        }
        foreach(var group in _groups) {
            group.RefreshLocalization(_localization);
        }
        foreach(var item in _items.Values) {
            item.RefreshLocalization(_localization);
        }
        RaisePropertyChanged(nameof(ProcessText));
        RaisePropertyChanged(nameof(GeneralMessage));
        if(LastContext is not null) {
            ContextSummary = string.Format(CultureInfo.CurrentCulture,
                _localization.GetLocalizedString("Manager.ContextSummary"), LastContext.DocumentTitle,
                LastContext.ViewName, LastContext.SelectionUniqueIds.Count);
        } else {
            ContextSummary = _localization.GetLocalizedString("Manager.Refresh");
        }
    }

    // Синхронизирует раскрытие CardExpander и прежнюю независимую проекцию строк.
    private void OnGroupExpansionChanged(ManagerGroupViewModel group) {
        DetailsDismissRequested?.Invoke();
        if(group.IsExpanded) {
            _expandedGroups.Add(group.Key);
        } else {
            _expandedGroups.Remove(group.Key);
        }
        if(group.Count > _maximumIncrementalChanges) {
            ProjectionChanging?.Invoke(_projectionScope);
            _visibleRows.ReplaceRows(CreateVisibleRows());
            ProjectionChanged?.Invoke(_projectionScope);
            return;
        }
        int index = _visibleRows.IndexOf(group) + 1;
        foreach(var item in group.Items) {
            if(group.IsExpanded) {
                _visibleRows.Insert(index++, item);
            } else {
                _visibleRows.RemoveAt(index);
            }
        }
    }

    // Отбрасывает команды со старым DataContext после замены или recycling строки.
    private bool ContainsElement(ManagerElementViewModel element) {
        return _elements.Contains(element);
    }

    // Повторное нажатие выбранного диапазона также обновляет данные существующей командой источника.
    private void SelectScope(ManagerScopeViewModel scope) {
        if(!Scopes.Contains(scope)) {
            return;
        }
        var command = scope.Scope switch {
            ManagerScope.View => ViewScopeCommand,
            ManagerScope.Selected => SelectedScopeCommand,
            _ => ProjectScopeCommand
        };
        if(command?.CanExecute(null) == true) {
            SelectedScope = scope;
            command.Execute(null);
        }
    }

    // Использует существующие флаги сессии и запроса, без второй машины состояний.
    private void NotifyPresentationState() {
        RaisePropertyChanged(nameof(CanChangeScope));
        RaisePropertyChanged(nameof(GeneralMessage));
        RaisePropertyChanged(nameof(HasGeneralMessage));
        if(!CanEditRows) {
            DetailsDismissRequested?.Invoke();
        }
    }

    // Обновляет итог на величину изменения одной группы, независимо от раскрытия.
    private void OnGroupStateChanged(ManagerGroupViewModel group, int previousCount) {
        SetProcessingCount(_processingCount + group.ProcessingCount - previousCount);
    }

    // Оповещает кнопку только после изменения агрегированного количества.
    private void SetProcessingCount(int count) {
        if(_processingCount == count) {
            return;
        }
        _processingCount = count;
        RaisePropertyChanged(nameof(ProcessingCount));
        RaisePropertyChanged(nameof(ProcessText));
    }

    // Собирает заголовки и только видимых детей в порядке текущего диапазона.
    private List<ManagerRowViewModel> CreateVisibleRows() {
        var rows = new List<ManagerRowViewModel>();
        foreach(var group in _groups) {
            rows.Add(group);
            if(group.IsExpanded) {
                rows.AddRange(group.Items);
            }
        }
        return rows;
    }

    // Передаёт небольшую дельту без Reset, а массовую замену публикует одним событием WPF.
    private void UpdateVisibleRows(IReadOnlyList<ManagerRowViewModel> rows) {
        var existingRows = new HashSet<ManagerRowViewModel>(_visibleRows);
        var targetRows = new HashSet<ManagerRowViewModel>(rows);
        int removed = _visibleRows.Count(row => !targetRows.Contains(row));
        int added = rows.Count(row => !existingRows.Contains(row));
        if(removed + added > _maximumIncrementalChanges) {
            _visibleRows.ReplaceRows(rows);
            return;
        }
        for(int index = _visibleRows.Count - 1; index >= 0; index--) {
            if(!targetRows.Contains(_visibleRows[index])) {
                _visibleRows.RemoveAt(index);
            }
        }
        for(int index = 0; index < rows.Count; index++) {
            if(index < _visibleRows.Count && ReferenceEquals(_visibleRows[index], rows[index])) {
                continue;
            }
            if(existingRows.Contains(rows[index])) {
                _visibleRows.Move(_visibleRows.IndexOf(rows[index]), index);
            } else {
                _visibleRows.Insert(index, rows[index]);
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
