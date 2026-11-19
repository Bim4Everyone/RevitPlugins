using dosymep.SimpleServices;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.ViewModels;

/// <summary>Отображает сведения элемента, не обращаясь к модели Revit.</summary>
internal sealed class ManagerElementViewModel(ManagerElementPresentation element, bool isOpening,
    ILocalizationService localization, bool showInformation = true) {
    public bool IsMissing => element is null;
    public bool ShowInformation => showInformation && !IsMissing;
    public string TypeName => element?.TypeName ?? localization.GetLocalizedString(
        isOpening ? "Manager.NoOpening" : "Manager.NoLintel");
    public ManagerElementStatus Status => element?.Status ?? ManagerElementStatus.Missing;
    public string StatusText => localization.GetLocalizedString("Manager.Status." + Status);
}
