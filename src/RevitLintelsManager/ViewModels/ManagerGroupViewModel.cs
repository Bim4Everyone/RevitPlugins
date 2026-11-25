using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using dosymep.SimpleServices;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.ViewModels;

/// <summary>Хранит исходную группу и агрегирует состояние её строк после фильтрации.</summary>
internal sealed class ManagerGroupViewModel : ManagerRowViewModel, IDisposable {
    public event Action<ManagerGroupViewModel, int, int> StateChanged;
    public event Action<ManagerGroupViewModel> ExpansionChanged;
    private readonly Dictionary<ManagerAction, int> _actionCounts = new();
    private readonly ILocalizationService _localization;
    private IReadOnlyList<ManagerItemViewModel> _filteredItems = Array.Empty<ManagerItemViewModel>();
    private IReadOnlyList<ManagerActionViewModel> _filteredActions = Array.Empty<ManagerActionViewModel>();
    private HashSet<ManagerItemViewModel> _filteredItemSet;
    private ManagerElementStatus _groupStatus = ManagerElementStatus.NotAnalyzed;
    private bool _isExpanded;
    private bool _isUpdating;
    private bool _disposed;
    private int _selectedCount;
    private int _processingCount;
    private bool? _isChecked;
    private ManagerActionViewModel _selectedAction;
    private bool _hasMixedActions;

    /// <summary>Подписывается на исходные строки и вычисляет агрегаты начального состава группы.</summary>
    public ManagerGroupViewModel(ManagerGroupKey key, ManagerElementPresentation opening,
        ManagerElementPresentation lintel, IReadOnlyList<ManagerItemViewModel> items,
        ILocalizationService localization)
        : base(CreateHeaderElement(opening), CreateHeaderElement(lintel),
            items.SelectMany(item => item.Actions.Select(action => action.Action)), localization, key,
            showInformation: false, correctionTitleKey: "Manager.Action.Correct.Group") {
        Key = key;
        _localization = localization;
        Items = items.ToList().AsReadOnly();
        if(Items.Distinct().Count() != Items.Count) {
            throw new ArgumentException("Дочерняя строка не должна повторяться в группе.", nameof(items));
        }
        foreach(var item in Items) {
            item.StateChanged += OnItemStateChanged;
        }
        SetFilteredItems(Items);
    }

    public ManagerGroupKey Key { get; }
    public IReadOnlyList<ManagerItemViewModel> Items { get; }
    public IReadOnlyList<ManagerItemViewModel> FilteredItems => _filteredItems;
    public override IReadOnlyList<ManagerActionViewModel> Actions => _filteredActions;
    public int Count => _filteredItems.Count;
    public int SelectedCount => _selectedCount;
    public int ProcessingCount => _processingCount;
    public ManagerElementStatus GroupStatus => _groupStatus;
    public string CountText => string.Format(CultureInfo.CurrentCulture,
        _localization.GetLocalizedString("Manager.Count"), Count);
    public bool IsExpanded {
        get => _isExpanded;
        set {
            if(_disposed || _isExpanded == value) {
                return;
            }
            RaiseAndSetIfChanged(ref _isExpanded, value);
            ExpansionChanged?.Invoke(this);
        }
    }
    public override string MixedActionsText => _hasMixedActions
        ? _localization.GetLocalizedString("Manager.Variants") : null;
    public override bool? IsChecked {
        get => _isChecked;
        set {
            if(_disposed || _isUpdating) {
                return;
            }
            UpdateItems(item => item.IsChecked = value ?? true);
        }
    }
    public override ManagerActionViewModel SelectedAction {
        get => _selectedAction;
        set {
            var action = Actions.FirstOrDefault(option => option.Action == value?.Action);
            if(_disposed || _isUpdating || action is null) {
                return;
            }
            // Неотмеченные строки в результате фильтра тоже получают групповое действие.
            UpdateItems(item => item.SelectedAction = item.Actions.First(option => option.Action == action.Action));
        }
    }

    /// <summary>Обновляет отображаемую часть группы и область её команд, сохраняя исходные строки.</summary>
    public void SetFilteredItems(IReadOnlyList<ManagerItemViewModel> items) {
        if(ReferenceEquals(_filteredItems, items) || _filteredItems.SequenceEqual(items)) {
            return;
        }
        int previousSelected = _selectedCount;
        int previousProcessing = _processingCount;
        if(_filteredItems.Count > 0) {
            _filteredItems[_filteredItems.Count - 1].IsLastInGroup = false;
        }
        _filteredItems = items;
        _filteredItemSet = ReferenceEquals(items, Items) ? null : new HashSet<ManagerItemViewModel>(items);
        _selectedCount = 0;
        _processingCount = 0;
        _actionCounts.Clear();
        foreach(var item in items) {
            _selectedCount += item.IsChecked == true ? 1 : 0;
            _processingCount += item.IsProcessingSelected ? 1 : 0;
            AddAction(item.CurrentAction, 1);
        }
        _isUpdating = true;
        try {
            UpdatePresentation();
            var actions = GetCommonActions(items);
            var filteredActions = base.Actions.Where(action => actions.Contains(action.Action)).ToList().AsReadOnly();
            bool actionsChanged = !_filteredActions.SequenceEqual(filteredActions);
            if(actionsChanged) {
                _filteredActions = filteredActions;
                RaisePropertyChanged(nameof(Actions));
            }
            PublishState(previousSelected, previousProcessing, actionsChanged);
        } finally {
            _isUpdating = false;
        }
    }

    /// <inheritdoc />
    public override void RefreshLocalization(ILocalizationService localization) {
        base.RefreshLocalization(localization);
        RaisePropertyChanged(nameof(CountText));
        RaisePropertyChanged(nameof(MixedActionsText));
    }

    /// <summary>Снимает подписки на детей при замене группы или освобождении менеджера.</summary>
    public void Dispose() {
        if(_disposed) {
            return;
        }
        _disposed = true;
        foreach(var item in Items) {
            item.StateChanged -= OnItemStateChanged;
        }
        StateChanged = null;
        ExpansionChanged = null;
    }

    // У заголовка остаётся статус типоразмера, а содержание предупреждений берётся из показанных строк.
    private static ManagerElementPresentation CreateHeaderElement(ManagerElementPresentation element) {
        return element is null ? null : new ManagerElementPresentation(element.TypeName, element.Status,
            element.Information, widthMm: element.WidthMm,
            heightMm: element.HeightMm, thicknessMm: element.ThicknessMm);
    }

    // Пересекает предоставленные наборы, не вычисляя предметные ограничения.
    private static HashSet<ManagerAction> GetCommonActions(IReadOnlyList<ManagerItemViewModel> items) {
        if(items.Count == 0) {
            return new HashSet<ManagerAction>();
        }
        var actions = new HashSet<ManagerAction>(items[0].Actions.Select(option => option.Action));
        foreach(var item in items.Skip(1)) {
            actions.IntersectWith(item.Actions.Select(option => option.Action));
        }
        return actions;
    }

    // Обновляет общую рамку, нижнюю границу и диагностику заголовка после смены состава.
    private void UpdatePresentation() {
        var status = Count > 0 && Key.IsFixed ? ManagerElementStatus.Fixed
            : _filteredItems.Any(item => item.ResultStatus == ManagerElementStatus.Warning) ? ManagerElementStatus.Warning
            : Count > 0 && _filteredItems.All(item => item.ResultStatus == ManagerElementStatus.Correct)
                ? ManagerElementStatus.Correct : ManagerElementStatus.NotAnalyzed;
        RaiseAndSetIfChanged(ref _groupStatus, status, nameof(GroupStatus));
        int lastIndex = Count - 1;
        for(int index = 0; index < Count; index++) {
            var item = _filteredItems[index];
            item.GroupStatus = status;
            item.IsLastInGroup = index == lastIndex;
        }
        Opening.SetRowDiagnostics(_filteredItems.SelectMany(item => item.Snapshot.Diagnostics
            .Concat(item.Opening.Element?.Diagnostics ?? Array.Empty<ManagerDiagnostic>())));
        Lintel.SetRowDiagnostics(_filteredItems.SelectMany(item => item.Snapshot.Diagnostics
            .Concat(item.Lintel.Element?.Diagnostics ?? Array.Empty<ManagerDiagnostic>())));
        RaisePropertyChanged(nameof(Count));
        RaisePropertyChanged(nameof(CountText));
    }

    // Меняет только счётчики затронутой группы; групповая операция публикует итог один раз.
    private void OnItemStateChanged(ManagerItemViewModel item, bool previousChecked, ManagerAction previousAction) {
        if(_filteredItemSet is not null && !_filteredItemSet.Contains(item)) {
            return;
        }
        int previousSelected = _selectedCount;
        int previousProcessing = _processingCount;
        _selectedCount += (item.IsChecked == true ? 1 : 0) - (previousChecked ? 1 : 0);
        _processingCount += (item.IsProcessingSelected ? 1 : 0)
            - (previousChecked && previousAction != ManagerAction.Skip ? 1 : 0);
        AddAction(previousAction, -1);
        AddAction(item.CurrentAction, 1);
        if(!_isUpdating) {
            PublishState(previousSelected, previousProcessing);
        }
    }

    // Объединяет групповые уведомления, сохраняя индивидуальные уведомления видимых строк.
    private void UpdateItems(Action<ManagerItemViewModel> update) {
        int previousSelected = _selectedCount;
        int previousProcessing = _processingCount;
        _isUpdating = true;
        try {
            foreach(var item in _filteredItems) {
                update(item);
            }
        } finally {
            _isUpdating = false;
            PublishState(previousSelected, previousProcessing);
        }
    }

    // Хранит частоты не более шести действий вместо повторного обхода всех детей.
    private void AddAction(ManagerAction action, int delta) {
        _actionCounts.TryGetValue(action, out int count);
        if(count + delta == 0) {
            _actionCounts.Remove(action);
        } else {
            _actionCounts[action] = count + delta;
        }
    }

    // Публикует агрегаты напрямую, не вызывая пользовательские setters и рекурсивное назначение.
    private void PublishState(int previousSelected, int previousProcessing, bool actionsChanged = false) {
        bool? isChecked = _selectedCount == 0 ? false : _selectedCount == Count ? true : null;
        RaiseAndSetIfChanged(ref _isChecked, isChecked, nameof(IsChecked));
        ManagerAction? common = _actionCounts.Count == 1 ? _actionCounts.Keys.First() : null;
        var selected = Actions.FirstOrDefault(option => option.Action == common);
        if(actionsChanged && ReferenceEquals(_selectedAction, selected)) {
            RaisePropertyChanged(nameof(SelectedAction));
        } else {
            RaiseAndSetIfChanged(ref _selectedAction, selected, nameof(SelectedAction));
        }
        bool mixed = _actionCounts.Count > 1;
        if(_hasMixedActions != mixed) {
            _hasMixedActions = mixed;
            RaisePropertyChanged(nameof(MixedActionsText));
        }
        if(previousProcessing != _processingCount) {
            RaisePropertyChanged(nameof(ProcessingCount));
        }
        if(previousSelected != _selectedCount || previousProcessing != _processingCount) {
            StateChanged?.Invoke(this, previousSelected, previousProcessing);
        }
    }
}
