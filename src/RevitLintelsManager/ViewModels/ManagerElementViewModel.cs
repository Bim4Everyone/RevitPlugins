using System.Collections.Generic;
using System.Linq;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.ViewModels;

/// <summary>Отображает сведения элемента, не обращаясь к модели Revit.</summary>
internal sealed class ManagerElementViewModel(ManagerElementPresentation element, bool isOpening,
    ILocalizationService localization, bool showInformation = true, bool analysisPending = false,
    ManagerElementStatus absentStatus = ManagerElementStatus.Missing, bool rowIsFixed = false) : BaseViewModel {
    public ManagerElementPresentation Element => element;
    public bool IsOpening => isOpening;
    public IReadOnlyList<ManagerDiagnostic> RowDiagnostics { get; private set; } =
        new List<ManagerDiagnostic>().AsReadOnly();
    public bool IsLintelNotRequired => !isOpening && element is null
        && absentStatus == ManagerElementStatus.LintelNotRequired;
    public bool IsLintelMissing => !isOpening && element is null && !IsLintelNotRequired && !HasOpeningConflict;
    public bool HasOpeningConflict => element is null && !IsAnalysisPending
        && absentStatus is ManagerElementStatus.MultipleOpenings or ManagerElementStatus.OpeningConflict;
    public bool IsStatusPlaceholder => IsLintelNotRequired || IsLintelMissing || HasOpeningConflict
        || (isOpening && element is null && !IsAnalysisPending && absentStatus == ManagerElementStatus.Missing);
    public bool IsFixed => rowIsFixed;
    public bool IsAnalysisPending => analysisPending;
    public bool ShowInformation => showInformation && element is not null;
    public bool ShowStatus => element is not null || !IsAnalysisPending;
    public string TypeName => element is null ? localization.GetLocalizedString(GetPlaceholderKey())
        : string.IsNullOrWhiteSpace(element.TypeName)
            ? localization.GetLocalizedString("Manager.Unavailable") : element.TypeName;
    public ManagerElementStatus Status => IsFixed ? ManagerElementStatus.Fixed : element?.Status
        ?? (IsAnalysisPending ? ManagerElementStatus.NotAnalyzed
            : HasOpeningConflict ? ManagerElementStatus.Warning : absentStatus);
    public string StatusText => IsLintelMissing && !IsAnalysisPending && !IsFixed ? TypeName
        : localization.GetLocalizedString("Manager.Status." + Status);
    public string InformationToolTip => localization.GetLocalizedString("Manager.Information");
    public string StatusToolTip => IsAnalysisPending || Status == ManagerElementStatus.Fixed ? StatusText
        : localization.GetLocalizedString(HasWarnings ? "Manager.HasWarnings" : "Manager.NoWarnings");
    public bool HasWarnings => !IsAnalysisPending && ((!IsLintelNotRequired && Status != ManagerElementStatus.Correct)
        || element?.Diagnostics.Count > 0
        || RowDiagnostics.Any(diagnostic => !diagnostic.IsInformational));

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

    // Выбирает подпись пустого поля по результату сопоставления.
    private string GetPlaceholderKey() {
        if(!isOpening) {
            return IsLintelNotRequired ? "Manager.LintelNotRequired" : "Manager.NoLintel";
        }
        return absentStatus switch {
            _ when IsAnalysisPending => "Manager.Unmatched",
            ManagerElementStatus.MultipleOpenings => "Manager.MultipleOpenings",
            ManagerElementStatus.Warning => "Manager.Unmatched",
            _ => "Manager.NoOpening"
        };
    }
}
