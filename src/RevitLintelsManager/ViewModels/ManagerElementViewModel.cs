using System.Collections.Generic;
using System.Linq;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.ViewModels;

/// <summary>Отображает сведения элемента, не обращаясь к модели Revit.</summary>
internal sealed class ManagerElementViewModel(ManagerElementPresentation element, bool isOpening,
    ILocalizationService localization, bool showInformation = true, bool analysisPending = false,
    bool counterpartUnconfirmed = false, bool rowIsFixed = false) : BaseViewModel {
    public ManagerElementPresentation Element => element;
    public bool IsOpening => isOpening;
    public IReadOnlyList<ManagerDiagnostic> RowDiagnostics { get; private set; } =
        new List<ManagerDiagnostic>().AsReadOnly();
    public bool IsLintelMissing => !isOpening && element is null;
    public bool IsMissing => IsLintelMissing || (element is null && !IsAnalysisPending && !counterpartUnconfirmed);
    public bool IsFixed => rowIsFixed;
    public bool IsAnalysisPending => analysisPending;
    public bool ShowInformation => showInformation && element is not null;
    public bool ShowStatus => element is not null || !IsAnalysisPending;
    public string TypeName => element is null
        ? localization.GetLocalizedString(!isOpening ? "Manager.NoLintel"
            : IsAnalysisPending || counterpartUnconfirmed ? "Manager.Unmatched" : "Manager.NoOpening")
        : string.IsNullOrWhiteSpace(element.TypeName)
            ? localization.GetLocalizedString("Manager.Unavailable") : element.TypeName;
    public ManagerElementStatus Status => IsFixed ? ManagerElementStatus.Fixed : element?.Status
        ?? (IsAnalysisPending ? ManagerElementStatus.NotAnalyzed
            : counterpartUnconfirmed ? ManagerElementStatus.Warning : ManagerElementStatus.Missing);
    public string StatusText => IsLintelMissing && !IsAnalysisPending && !IsFixed ? TypeName
        : localization.GetLocalizedString("Manager.Status." + Status);
    public string InformationToolTip => localization.GetLocalizedString("Manager.Information");
    public string StatusToolTip => IsAnalysisPending || IsFixed ? StatusText : localization.GetLocalizedString(
        HasWarnings ? "Manager.HasWarnings" : "Manager.NoWarnings");
    public bool HasWarnings => !IsAnalysisPending && (Status != ManagerElementStatus.Correct
        || element?.Diagnostics.Count > 0 || RowDiagnostics.Count > 0);

    /// <summary>Привязывает подготовленную диагностику строки, не приписывая её конкретному элементу.</summary>
    /// <param name="diagnostics">Диагностика пары или агрегированного заголовка.</param>
    public void SetRowDiagnostics(IEnumerable<ManagerDiagnostic> diagnostics) {
        RowDiagnostics = diagnostics.ToList().AsReadOnly();
        RaisePropertyChanged(nameof(HasWarnings));
        RaisePropertyChanged(nameof(StatusToolTip));
    }

    /// <summary>Обновляет локализованные подписи без изменения имён элементов.</summary>
    public void RefreshLocalization() {
        RaisePropertyChanged(nameof(TypeName));
        RaisePropertyChanged(nameof(StatusText));
        RaisePropertyChanged(nameof(InformationToolTip));
        RaisePropertyChanged(nameof(StatusToolTip));
    }
}
