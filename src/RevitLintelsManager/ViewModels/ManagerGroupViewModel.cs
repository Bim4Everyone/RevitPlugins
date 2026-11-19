using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using dosymep.SimpleServices;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.ViewModels;

/// <summary>Агрегирует выбор и действия всех детей независимо от визуальных контейнеров.</summary>
internal sealed class ManagerGroupViewModel : ManagerRowViewModel, IDisposable {
    public event Action<ManagerGroupViewModel, int> StateChanged;
    private readonly Dictionary<ManagerAction, int> _actionCounts = new();
    private readonly ILocalizationService _localization;
    private bool _isExpanded;
    private bool _isUpdating;
    private bool _disposed;
    private int _selectedCount;
    private int _processingCount;
    private bool? _isChecked;
    private ManagerActionViewModel _selectedAction;
    private bool _hasMixedActions;

    /// <summary>Вычисляет пересечение готовых AllowedActions и подписывается на состояние детей.</summary>
    public ManagerGroupViewModel(ManagerGroupKey key, ManagerElementPresentation opening,
        ManagerElementPresentation lintel, IReadOnlyList<ManagerItemViewModel> items,
        ILocalizationService localization)
        : base(opening, lintel, GetCommonActions(items), localization, false) {
        Key = key;
        _localization = localization;
        Items = items.ToList().AsReadOnly();
        if(Items.Distinct().Count() != Items.Count) {
            throw new ArgumentException("Дочерняя строка не должна повторяться в группе.", nameof(items));
        }
        CountText = string.Format(CultureInfo.CurrentCulture,
            localization.GetLocalizedString("Manager.Count"), Items.Count);
        foreach(var item in Items) {
            _selectedCount += item.IsChecked == true ? 1 : 0;
            _processingCount += item.IsProcessingSelected ? 1 : 0;
            AddAction(item.CurrentAction, 1);
            item.StateChanged += OnItemStateChanged;
        }
        PublishState(_processingCount);
    }

    public ManagerGroupKey Key { get; }
    public IReadOnlyList<ManagerItemViewModel> Items { get; }
    public int Count => Items.Count;
    public int ProcessingCount => _processingCount;
    public string CountText { get; }
    public bool IsExpanded {
        get => _isExpanded;
        internal set => RaiseAndSetIfChanged(ref _isExpanded, value);
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
            // Назначение действует также на снятых checkbox детей.
            UpdateItems(item => item.SelectedAction = item.Actions.First(option => option.Action == action.Action));
        }
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
    }

    // Пересекает предоставленные наборы, не вычисляя предметные ограничения.
    private static IEnumerable<ManagerAction> GetCommonActions(IReadOnlyList<ManagerItemViewModel> items) {
        if(items is null) {
            throw new ArgumentNullException(nameof(items));
        }
        if(items.Count == 0) {
            return Array.Empty<ManagerAction>();
        }
        var actions = new HashSet<ManagerAction>(items[0].Actions.Select(option => option.Action));
        foreach(var item in items.Skip(1)) {
            actions.IntersectWith(item.Actions.Select(option => option.Action));
        }
        return items[0].Actions.Select(option => option.Action).Where(actions.Contains).ToList();
    }

    // Меняет только счётчики затронутой группы; групповая операция публикует итог один раз.
    private void OnItemStateChanged(ManagerItemViewModel item, bool previousChecked, ManagerAction previousAction) {
        int previousCount = _processingCount;
        _selectedCount += (item.IsChecked == true ? 1 : 0) - (previousChecked ? 1 : 0);
        _processingCount += (item.IsProcessingSelected ? 1 : 0)
            - (previousChecked && previousAction != ManagerAction.Skip ? 1 : 0);
        AddAction(previousAction, -1);
        AddAction(item.CurrentAction, 1);
        if(!_isUpdating) {
            PublishState(previousCount);
        }
    }

    // Объединяет групповые уведомления, сохраняя индивидуальные уведомления видимых строк.
    private void UpdateItems(Action<ManagerItemViewModel> update) {
        int previousCount = _processingCount;
        _isUpdating = true;
        try {
            foreach(var item in Items) {
                update(item);
            }
        } finally {
            _isUpdating = false;
            PublishState(previousCount);
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
    private void PublishState(int previousCount) {
        bool? isChecked = _selectedCount == 0 ? false : _selectedCount == Count ? true : null;
        RaiseAndSetIfChanged(ref _isChecked, isChecked, nameof(IsChecked));
        ManagerAction? common = _actionCounts.Count == 1 ? _actionCounts.Keys.First() : null;
        var selected = Actions.FirstOrDefault(option => option.Action == common);
        RaiseAndSetIfChanged(ref _selectedAction, selected, nameof(SelectedAction));
        bool mixed = _actionCounts.Count > 1;
        if(_hasMixedActions != mixed) {
            _hasMixedActions = mixed;
            RaisePropertyChanged(nameof(MixedActionsText));
        }
        if(previousCount != _processingCount) {
            RaisePropertyChanged(nameof(ProcessingCount));
            StateChanged?.Invoke(this, previousCount);
        }
    }
}
