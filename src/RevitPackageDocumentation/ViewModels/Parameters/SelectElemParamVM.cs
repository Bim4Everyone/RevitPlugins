using Autodesk.Revit.DB;

using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters;

namespace RevitPackageDocumentation.ViewModels.Parameters;
internal class SelectElemParamVM : PluginParamVM {
    private Element _selectedElem;

    public SelectElemParamVM(SheetSetParametersListVM sheetSetParamsList, string paramName, string paramComment)
        : base(sheetSetParamsList, paramName, paramComment) {
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
