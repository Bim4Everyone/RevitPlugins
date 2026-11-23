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
        bool analysisPending = opening?.Status == ManagerElementStatus.NotAnalyzed
            || lintel?.Status == ManagerElementStatus.NotAnalyzed;
        Opening = new ManagerElementViewModel(opening, true, localization, showInformation, analysisPending);
        Lintel = new ManagerElementViewModel(lintel, false, localization, showInformation, analysisPending);
        Actions = actions.Distinct().Select(value => new ManagerActionViewModel(value,
            localization.GetLocalizedString("Manager.Action." + value))).ToList().AsReadOnly();
    }

    public ManagerElementViewModel Opening { get; }
    public ManagerElementViewModel Lintel { get; }
    public IReadOnlyList<ManagerActionViewModel> Actions { get; }
    public abstract ManagerActionViewModel SelectedAction { get; set; }
    public virtual string MixedActionsText => null;
    public abstract bool? IsChecked { get; set; }

    /// <summary>Обновляет подписи, сохраняя внутренние enum и назначенные действия.</summary>
    /// <param name="localization">Существующий сервис локализации.</param>
    public virtual void RefreshLocalization(ILocalizationService localization) {
        Opening.RefreshLocalization();
        Lintel.RefreshLocalization();
        foreach(var action in Actions) {
            action.RefreshLocalization(localization);
        }
    }
}

/// <summary>Подпись операции; «Варианты» не входит в список операций.</summary>
internal sealed class ManagerActionViewModel(ManagerAction action, string title) : BaseViewModel {
    private string _title = title;
    public ManagerAction Action { get; } = action;
    public string Title => _title;

    /// <summary>Обновляет только подпись операции.</summary>
    /// <param name="localization">Существующий сервис локализации.</param>
    public void RefreshLocalization(ILocalizationService localization) {
        RaiseAndSetIfChanged(ref _title, localization.GetLocalizedString("Manager.Action." + Action), nameof(Title));
    }
}
