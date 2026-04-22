using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.ViewModels;

internal class ParameterViewModel {
    private readonly string _name;
    private readonly FilterableParam _filterableParam;
    private readonly RevitParam _revitParam;

    public ParameterViewModel(FilterableParam param) {
        _filterableParam = param;
        _revitParam = param.RevitParam;
        _name = param.RevitParam.Name;
    }

    public string Name => _name;
    public FilterableParam FilterableParam => _filterableParam;
    public RevitParam RevitParam => _revitParam;
}
