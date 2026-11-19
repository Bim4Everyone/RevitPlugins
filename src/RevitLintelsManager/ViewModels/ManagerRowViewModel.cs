using System.Collections.Generic;
using System.Linq;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.ViewModels;

/// <summary>Общие поля строки без предметного расчёта допустимости действий.</summary>
internal abstract class ManagerRowViewModel : BaseViewModel {
    /// <summary>Материализует только данные представления и локализованные подписи действий.</summary>
    protected ManagerRowViewModel(ManagerElementPresentation opening, ManagerElementPresentation lintel,
        IEnumerable<ManagerAction> actions,
        ILocalizationService localization, bool showInformation = true) {
        Opening = new ManagerElementViewModel(opening, true, localization, showInformation);
        Lintel = new ManagerElementViewModel(lintel, false, localization, showInformation);
        Actions = actions.Distinct().Select(value => new ManagerActionViewModel(value,
            localization.GetLocalizedString("Manager.Action." + value))).ToList().AsReadOnly();
    }

    public ManagerElementViewModel Opening { get; }
    public ManagerElementViewModel Lintel { get; }
    public IReadOnlyList<ManagerActionViewModel> Actions { get; }
    public abstract ManagerActionViewModel SelectedAction { get; set; }
    public virtual string MixedActionsText => null;
    public abstract bool? IsChecked { get; set; }
}

/// <summary>Подпись операции; «Варианты» не входит в список операций.</summary>
internal sealed class ManagerActionViewModel(ManagerAction action, string title) {
    public ManagerAction Action { get; } = action;
    public string Title { get; } = title;
}
