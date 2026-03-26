using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

namespace RevitMarkAllDocuments.ViewModels;

internal class ParameterViewModel {
    private readonly string _name;
    private readonly RevitParam _revitParam;

    public ParameterViewModel(RevitParam param) {
        _revitParam = param;
        _name = param.Name;
    }

    public string Name => _name;
    public RevitParam RevitParam => _revitParam;
}
