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
    private readonly Dictionary<ManagerItemKey, ManagerItemState> _hiddenState = new();
    private readonly HashSet<ManagerItemKey> _deletionCandidates = new();
    private Dictionary<ManagerItemKey, ManagerItemViewModel> _items = new();
    private HashSet<ManagerElementViewModel> _elements = new();
    private bool _isBlocked;
    private bool _isRefreshing;
    private bool _firstRead = true;
    private bool _modelIsDirty;
    private string _contextSummary;
    private string _readError;
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
            element => ShowDetails(element, false),
            element => CanEditRows && element.ShowInformation && ContainsElement(element));
        ShowWarningsCommand = commands.Create<ManagerElementViewModel>(
            element => ShowDetails(element, true),
            element => CanEditRows && element.ShowStatus && ContainsElement(element));
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
    public bool CanChangeScope => CanEditRows;
    public int ProcessingCount => _processingCount;
    public string ProcessText => string.Format(CultureInfo.CurrentCulture,
        _localization.GetLocalizedString("Manager.ProcessCount"), ProcessingCount);
    public bool CanEditRows => !IsBlocked && !State.IsClosing && !IsRefreshing;
    public string GeneralMessage => State.IsClosing ? _localization.GetLocalizedString("Manager.Closing")
        : IsBlocked ? _localization.GetLocalizedString("Manager.DocumentChanged")
        : IsRefreshing ? _localization.GetLocalizedString("Manager.Busy")
        : _readError is not null ? GetReadErrorMessage()
        : _modelIsDirty ? _localization.GetLocalizedString("Manager.DataOutdated") : GetProjectMessage();
    public bool HasGeneralMessage => GeneralMessage is not null;
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

    /// <summary>Помечает отображаемые данные устаревшими и запоминает кандидатов на удаление.</summary>
    public void InvalidateModel(IEnumerable<long> deletedIds) {
        var deleted = new HashSet<long>(deletedIds);
        if(deleted.Count > 0) {
            CollectDeletionCandidates(deleted);
        }
        if(!_firstRead && !_modelIsDirty) {
            _modelIsDirty = true;
            RaisePropertyChanged(nameof(GeneralMessage));
            RaisePropertyChanged(nameof(HasGeneralMessage));
        }
    }

    /// <summary>Сверяет прочитанные строки и публикует группы выбранного диапазона.</summary>
    /// <param name="items">Строки, полученные от источника для сверки.</param>
    /// <param name="scope">Диапазон публикуемой проекции.</param>
    /// <param name="retainUnseen">Сохранять состояние строк вне прочитанной выборки.</param>
    public void SetItems(IReadOnlyList<ManagerItemViewModel> items, ManagerScope scope, bool retainUnseen = false) {
        ReconcileItemStates(items, retainUnseen);

        var previousGroups = _groups.ToDictionary(group => group.Key);
        var groups = new List<ManagerGroupViewModel>();
        foreach(var group in items.GroupBy(item => item.Snapshot.GroupKey)) {
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
        if(scope != _projectionScope) {
            DetailsDismissRequested?.Invoke();
        }
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

    // Хранит объекты представления только для текущей выборки и восстанавливает состояние по ключу строки.
    private void ReconcileItemStates(IReadOnlyList<ManagerItemViewModel> items, bool retainUnseen) {
        var replacement = items.ToDictionary(item => item.Snapshot.Key);
        foreach(var item in items) {
            var key = item.Snapshot.Key;
            if(_items.TryGetValue(key, out var previous)) {
                if(!ReferenceEquals(previous, item)) {
                    item.RestoreState(previous.CaptureState());
                }
            } else if(_hiddenState.TryGetValue(key, out var state)) {
                item.RestoreState(state);
                _hiddenState.Remove(key);
            }
        }
        if(retainUnseen) {
            foreach(var previous in _items) {
                if(!replacement.ContainsKey(previous.Key)) {
                    _hiddenState[previous.Key] = previous.Value.CaptureState();
                }
            }
        } else {
            _hiddenState.Clear();
        }
        _items = replacement;
        _expandedGroups.IntersectWith(items.Select(item => item.Snapshot.GroupKey)
            .Concat(_hiddenState.Values.Select(state => state.GroupKey)));
    }

    // Сопоставляет числовые ID из события с известными ключами, которые проверит следующий запрос.
    private void CollectDeletionCandidates(HashSet<long> deleted) {
        foreach(var row in _items) {
            if(IsDeletionCandidate(row.Value.CaptureState(), deleted)) {
                _deletionCandidates.Add(row.Key);
            }
        }
        foreach(var row in _hiddenState) {
            if(IsDeletionCandidate(row.Value, deleted)) {
                _deletionCandidates.Add(row.Key);
            }
        }
    }

    // У пары достаточно проверить исчезновение любого из её элементов.
    private static bool IsDeletionCandidate(ManagerItemState state, HashSet<long> deleted) {
        return (state.OpeningId is long openingId && deleted.Contains(openingId))
            || (state.LintelId is long lintelId && deleted.Contains(lintelId));
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
        _hiddenState.Clear();
        _deletionCandidates.Clear();
        _elements.Clear();
        _expandedGroups.Clear();
        _visibleRows.Clear();
        LastContext = null;
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
        RaisePropertyChanged(nameof(HasGeneralMessage));
        if(_readError is not null) {
            ContextSummary = GetReadErrorMessage();
        } else if(LastContext is not null) {
            ContextSummary = FormatContextSummary(LastContext);
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

    // Запоминает устойчивый ключ строки, из которой открыто мини-окно.
    private void ShowDetails(ManagerElementViewModel element, bool warnings) {
        var row = _visibleRows.FirstOrDefault(row => ReferenceEquals(row.Opening, element)
            || ReferenceEquals(row.Lintel, element));
        if(row is not null) {
            DetailsRequested?.Invoke(new ManagerDetailsViewModel(element, warnings, _localization,
                (row as ManagerItemViewModel)?.Snapshot.Key, (row as ManagerGroupViewModel)?.Key));
        }
    }

    /// <summary>Находит актуальные сведения мини-окна или возвращает null для исчезнувшей строки.</summary>
    public ManagerDetailsViewModel UpdateDetails(ManagerDetailsViewModel previous) {
        var row = FindVisibleRow(previous.ItemKey, previous.GroupKey);
        var element = previous.IsOpening ? row?.Opening : row?.Lintel;
        bool canShow = !IsBlocked && !State.IsClosing
            && (previous.IsWarnings ? element?.ShowStatus == true : element?.ShowInformation == true);
        return canShow ? new ManagerDetailsViewModel(element, previous.IsWarnings, _localization,
            previous.ItemKey, previous.GroupKey) : null;
    }

    /// <summary>Находит видимую строку для восстановления выбора и мини-окна.</summary>
    public ManagerRowViewModel FindVisibleRow(ManagerItemKey? itemKey, ManagerGroupKey? groupKey) {
        if(itemKey is ManagerItemKey item && _items.TryGetValue(item, out var row)) {
            return _expandedGroups.Contains(row.Snapshot.GroupKey) ? row : null;
        }
        return groupKey is ManagerGroupKey group
            ? _groups.FirstOrDefault(current => current.Key.Equals(group)) : null;
    }

    // Повторное нажатие выбранного диапазона также запускает чтение.
    private void SelectScope(ManagerScopeViewModel scope) {
        if(!Scopes.Contains(scope) || !RefreshCommand.CanExecute(null)) {
            return;
        }
        SelectedScope = scope;
        RefreshCommand.Execute(null);
    }

    // Использует существующие флаги сессии и запроса, без второй машины состояний.
    private void NotifyPresentationState() {
        RaisePropertyChanged(nameof(CanChangeScope));
        RaisePropertyChanged(nameof(GeneralMessage));
        RaisePropertyChanged(nameof(HasGeneralMessage));
        if(IsBlocked || State.IsClosing) {
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

    // Передаёт в очередь диапазон и идентичность запроса, публикует независимый ответ на UI Dispatcher.
    private async Task RefreshContextAsync() {
        IsRefreshing = true;
        CommandManager.InvalidateRequerySuggested();
        var scope = SelectedScope?.Scope ?? State.InitialScope;
        var request = new ManagerRequest(State.DocumentToken, State.AdvanceGeneration(), scope,
            _firstRead && scope == State.InitialScope, _deletionCandidates);
        _firstRead = false;
        try {
            var context = await _requests.Enqueue(request).ConfigureAwait(false);
            if(!_dispatcher.HasShutdownStarted) {
                await _dispatcher.InvokeAsync(() => ApplyContext(context, scope)).Task.ConfigureAwait(false);
            }
        } catch(Exception exception) when(exception is InvalidOperationException or OperationCanceledException) {
            if(!_dispatcher.HasShutdownStarted) {
                await _dispatcher.InvokeAsync(() => {
                    if(IsCurrentResponse(request.DocumentToken, request.Generation)) {
                        DetailsDismissRequested?.Invoke();
                        if(_projectionScope != scope) {
                            SetItems(Array.Empty<ManagerItemViewModel>(), scope, retainUnseen: true);
                        }
                        _readError = exception is OperationCanceledException
                            ? _localization.GetLocalizedString("Manager.ReadFailed") : exception.Message;
                        ContextSummary = GetReadErrorMessage();
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
    private void ApplyContext(ManagerDocumentContext context, ManagerScope scope) {
        if(!IsCurrentResponse(context.DocumentToken, context.Generation)) {
            return;
        }
        if(context.Project is not null) {
            foreach(var key in context.RemovedKeys) {
                _items.Remove(key);
                _hiddenState.Remove(key);
            }
            _deletionCandidates.Clear();
            var items = new List<ManagerItemViewModel>(context.Project.Openings.Count + context.Project.Lintels.Count);
            items.AddRange(context.Project.Openings.Select(opening => CreateProjectRow(context.DocumentToken,
                opening.Element, true, opening.WidthMm, heightMm: opening.HeightMm)));
            items.AddRange(context.Project.Lintels.Select(lintel => CreateProjectRow(context.DocumentToken,
                lintel.Element, false, lintel.OpeningWidthMm, thicknessMm: lintel.WallThicknessMm)));
            bool retainUnseen = scope != ManagerScope.Project || context.Project.UnreadableCount > 0;
            SetItems(items, scope, retainUnseen);
        }
        _readError = null;
        _modelIsDirty = false;
        LastContext = context;
        ContextSummary = FormatContextSummary(context);
    }

    // Проецирует одиночный экземпляр без вывода о наличии или отсутствии второй части пары.
    private ManagerItemViewModel CreateProjectRow(Guid documentToken, ManagerFamilySnapshot element,
        bool isOpening, double? widthMm, double? heightMm = null, double? thicknessMm = null) {
        const ManagerElementStatus status = ManagerElementStatus.NotAnalyzed;
        var key = new ManagerItemKey(documentToken, isOpening ? element.UniqueId : null,
            isOpening ? null : element.UniqueId);
        var group = new ManagerGroupKey(isOpening ? element.TypeUniqueId : null,
            isOpening ? null : element.TypeUniqueId, status, status, isOpening ? null : element.Information.IsFixed);
        if(_items.TryGetValue(key, out var previous) && previous.Snapshot.GroupKey.Equals(group)) {
            var old = isOpening ? previous.Opening.Element : previous.Lintel.Element;
            if(old.TypeName == element.TypeName && old.WidthMm == widthMm && old.HeightMm == heightMm
                && old.ThicknessMm == thicknessMm && old.Information.HasSameValues(element.Information)) {
                return previous;
            }
        }
        var presentation = new ManagerElementPresentation(element.TypeName, status, element.Information,
            widthMm: widthMm, heightMm: heightMm, thicknessMm: thicknessMm);
        var snapshot = new ManagerItemSnapshot(key, group, Array.Empty<ManagerDiagnostic>(),
            new[] { ManagerAction.Skip }, ManagerAction.Skip);
        return new ManagerItemViewModel(snapshot, isOpening ? presentation : null,
            isOpening ? null : presentation, _localization);
    }

    // Сообщает о неполной выборке или о подтверждённом отсутствии подходящих экземпляров.
    private string GetProjectMessage() {
        var project = LastContext?.Project;
        if(project is null) {
            return null;
        }
        if(project.UnreadableCount > 0) {
            return string.Format(CultureInfo.CurrentCulture, _localization.GetLocalizedString("Manager.ReadPartial"),
                project.UnreadableCount);
        }
        if(project.Openings.Count > 0 || project.Lintels.Count > 0) {
            return null;
        }
        bool isScoped = LastContext.ScopeStamp?.Scope is ManagerScope.View or ManagerScope.Selected;
        return _localization.GetLocalizedString(isScoped ? "Manager.ScopeEmpty" : "Manager.ProjectEmpty");
    }

    // Добавляет явное предупреждение, только если показана предыдущая выборка.
    private string GetReadErrorMessage() {
        return _groups.Count > 0
            ? _readError + " " + _localization.GetLocalizedString("Manager.PreviousData") : _readError;
    }

    // Подписывает фактическую выборку текущего диапазона.
    private string FormatContextSummary(ManagerDocumentContext context) {
        var project = context.Project;
        return string.Format(CultureInfo.CurrentCulture, _localization.GetLocalizedString("Manager.ProjectSummary"),
            context.DocumentTitle, project?.Openings.Count ?? 0, project?.Lintels.Count ?? 0);
    }

    // Проверяет актуальность успешного ответа и ошибки в момент их применения на UI-потоке.
    private bool IsCurrentResponse(Guid documentToken, long generation) {
        return !State.IsClosing && !State.IsBlocked && documentToken == State.DocumentToken
            && generation == State.Generation;
    }
}
