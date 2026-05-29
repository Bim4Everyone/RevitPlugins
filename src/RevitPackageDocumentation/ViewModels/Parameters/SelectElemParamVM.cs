using Autodesk.Revit.DB;

using RevitPackageDocumentation.ViewModels.Configuration;

namespace RevitPackageDocumentation.ViewModels.Parameters;
internal class SelectElemParamVM : PluginParamVM {
    private Element _selectedElem;

    public SelectElemParamVM(SheetSetVM sheetSetVM, string paramName, string paramComment)
        : base(sheetSetVM, paramName, paramComment) {
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
