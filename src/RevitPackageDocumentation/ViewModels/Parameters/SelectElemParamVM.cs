using Autodesk.Revit.DB;

namespace RevitPackageDocumentation.ViewModels.Parameters;
internal class SelectElemParamVM : PluginParamVM {
    private Element _selectedElem;

    public Element SelectedElem {
        get => _selectedElem;
        set => RaiseAndSetIfChanged(ref _selectedElem, value);
    }
}
