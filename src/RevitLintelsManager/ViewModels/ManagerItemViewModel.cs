using dosymep.SimpleServices;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.ViewModels;

/// <summary>Индивидуальная строка на независимом снимке без операций модели.</summary>
internal sealed class ManagerItemViewModel(ManagerItemSnapshot snapshot, ManagerElementPresentation opening,
    ManagerElementPresentation lintel, ILocalizationService localization)
    : ManagerRowViewModel(opening, lintel, snapshot.AllowedActions, snapshot.DefaultAction, true, localization) {
    public ManagerItemSnapshot Snapshot { get; } = snapshot;
}
