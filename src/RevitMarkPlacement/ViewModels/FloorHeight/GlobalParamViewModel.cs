using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitMarkPlacement.Extensions;

namespace RevitMarkPlacement.ViewModels.FloorHeight;

internal class GlobalParamViewModel : BaseViewModel {
    private readonly GlobalParameter _globalParameter;

    public GlobalParamViewModel(GlobalParameter globalParameter) {
        _globalParameter = globalParameter;
    }

    public ElementId Id => _globalParameter.Id;
    public string Name => _globalParameter.Name;
    public double Value => _globalParameter.AsDouble();
}
