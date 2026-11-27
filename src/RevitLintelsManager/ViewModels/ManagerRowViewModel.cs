using System.Collections.Generic;
using System.Linq;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.ViewModels;

/// <summary>Общие поля строки без предметного расчёта допустимости действий.</summary>
internal abstract class ManagerRowViewModel : BaseViewModel {
    private readonly IReadOnlyList<ManagerActionViewModel> _actions;

    /// <summary>Материализует только данные представления и локализованные подписи действий.</summary>
    protected ManagerRowViewModel(ManagerElementPresentation opening, ManagerElementPresentation lintel,
        IEnumerable<ManagerAction> actions,
        ILocalizationService localization, ManagerGroupKey groupKey, bool showInformation = true,
        string correctionTitleKey = null) {
        bool analysisPending = opening?.Status == ManagerElementStatus.NotAnalyzed
            || lintel?.Status == ManagerElementStatus.NotAnalyzed;
        Opening = new ManagerElementViewModel(opening, true, localization, showInformation, analysisPending,
            groupKey.OpeningStatus, rowIsFixed: groupKey.IsFixed);
        Lintel = new ManagerElementViewModel(lintel, false, localization, showInformation, analysisPending,
            groupKey.LintelStatus, rowIsFixed: groupKey.IsFixed);
        _actions = actions.Distinct().Select(value => {
            string key = value == ManagerAction.Correct && correctionTitleKey is not null
                ? correctionTitleKey : "Manager.Action." + value;
            return new ManagerActionViewModel(value, key, localization);
        }).ToList().AsReadOnly();
    }

    public ManagerElementViewModel Opening { get; }
    public ManagerElementViewModel Lintel { get; }
    public virtual IReadOnlyList<ManagerActionViewModel> Actions => _actions;
    public abstract ManagerActionViewModel SelectedAction { get; set; }
    public virtual string MixedActionsText => null;
    public abstract bool? IsChecked { get; set; }

    /// <summary>Обновляет подписи, сохраняя внутренние enum и назначенные действия.</summary>
    /// <param name="localization">Существующий сервис локализации.</param>
    public virtual void RefreshLocalization(ILocalizationService localization) {
        Opening.RefreshLocalization();
        Lintel.RefreshLocalization();
        foreach(var action in _actions) {
            action.RefreshLocalization(localization);
        }
    }
}

/// <summary>Подпись операции; «Варианты» не входит в список операций.</summary>
internal sealed class ManagerActionViewModel(ManagerAction action, string titleKey, ILocalizationService localization)
    : BaseViewModel {
    private string _title = localization.GetLocalizedString(titleKey);
    public ManagerAction Action { get; } = action;
    public string Title => _title;

    /// <summary>Обновляет только подпись операции.</summary>
    /// <param name="localization">Существующий сервис локализации.</param>
    public void RefreshLocalization(ILocalizationService localization) {
        RaiseAndSetIfChanged(ref _title, localization.GetLocalizedString(titleKey), nameof(Title));
    }
}
