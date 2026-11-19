using System.Collections.Generic;
using System.Linq;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.ViewModels;

/// <summary>Общее представление полей строки; назначение действий появится в TASK-004.</summary>
internal abstract class ManagerRowViewModel : BaseViewModel {
    /// <summary>Материализует только данные представления и локализованные подписи действий.</summary>
    protected ManagerRowViewModel(ManagerElementPresentation opening, ManagerElementPresentation lintel,
        IEnumerable<ManagerAction> actions, ManagerAction? action, bool? isChecked,
        ILocalizationService localization, bool showInformation = true) {
        Opening = new ManagerElementViewModel(opening, true, localization, showInformation);
        Lintel = new ManagerElementViewModel(lintel, false, localization, showInformation);
        Actions = actions.Select(value => new ManagerActionViewModel(value,
            localization.GetLocalizedString("Manager.Action." + value))).ToList().AsReadOnly();
        SelectedAction = Actions.FirstOrDefault(value => value.Action == action);
        MixedActionsText = action is null ? localization.GetLocalizedString("Manager.Variants") : null;
        IsChecked = isChecked;
    }

    public ManagerElementViewModel Opening { get; }
    public ManagerElementViewModel Lintel { get; }
    public IReadOnlyList<ManagerActionViewModel> Actions { get; }
    public ManagerActionViewModel SelectedAction { get; }
    public string MixedActionsText { get; }
    public bool? IsChecked { get; }
}

/// <summary>Подпись операции; «Варианты» не входит в список операций.</summary>
internal sealed class ManagerActionViewModel(ManagerAction action, string title) {
    public ManagerAction Action { get; } = action;
    public string Title { get; } = title;
}
