using Autodesk.Revit.DB;

namespace RevitPackageDocumentation.ViewModels.Parameters;
internal class SelectElemParamVM : PluginParamVM {
    private Element _selectedElem;

    public SelectElemParamVM(string paramName, string paramComment) : base(paramName, paramComment) {
        ValidateParamValue();
    }

    public Element SelectedElem {
        get => _selectedElem;
        set => RaiseAndSetIfChanged(ref _selectedElem, value);
    }

    public override void ValidateParamValue() {
        ErrorInParamValue = SelectedElem is null;
    }
}
