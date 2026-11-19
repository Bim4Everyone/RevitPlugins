using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using dosymep.SimpleServices;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.ViewModels;

/// <summary>Заголовок группы и невизуальные дочерние строки; агрегирование отложено до TASK-004.</summary>
internal sealed class ManagerGroupViewModel : ManagerRowViewModel {
    private bool _isExpanded;

    /// <summary>Принимает готовые сведения заголовка; не вычисляет допустимость групповых действий.</summary>
    public ManagerGroupViewModel(ManagerGroupKey key, ManagerElementPresentation opening,
        ManagerElementPresentation lintel, IEnumerable<ManagerItemViewModel> items,
        IEnumerable<ManagerAction> actions, ManagerAction? action, bool? isChecked,
        ILocalizationService localization)
        : base(opening, lintel, actions, action, isChecked, localization, false) {
        Key = key;
        Items = items.ToList().AsReadOnly();
        CountText = string.Format(CultureInfo.CurrentCulture,
            localization.GetLocalizedString("Manager.Count"), Items.Count);
    }

    public ManagerGroupKey Key { get; }
    public IReadOnlyList<ManagerItemViewModel> Items { get; }
    public int Count => Items.Count;
    public bool IsExpanded {
        get => _isExpanded;
        internal set => RaiseAndSetIfChanged(ref _isExpanded, value);
    }
    public string CountText { get; }
}
