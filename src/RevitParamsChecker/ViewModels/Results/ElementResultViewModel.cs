using dosymep.WPF.ViewModels;

using Autodesk.Revit.DB;

using RevitParamsChecker.Models.Revit;

namespace RevitParamsChecker.ViewModels.Results;

internal class ElementResultViewModel : BaseViewModel {
    public ElementResultViewModel() {
        // TODO
    }

    public ElementId Id { get; } = ElementId.InvalidElementId;

    public string FileName { get; } = "File 1";

    public string Status { get; } = "Status";

    public string Error { get; } = "Error about smth";

    public string RuleName { get; } = "Rule 1";

    public ElementModel ElementModel { get; }
}
