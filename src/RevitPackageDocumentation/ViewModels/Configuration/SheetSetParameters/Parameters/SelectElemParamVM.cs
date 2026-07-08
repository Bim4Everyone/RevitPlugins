using System.ComponentModel.DataAnnotations;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
internal class SelectElemParamVM : PluginParamVM {
    private Element _selectedElem;

    public SelectElemParamVM(
        SheetSetParametersListVM sheetSetParamsList,
        string paramName,
        string paramComment,
        ILocalizationService localizationService)
        : base(sheetSetParamsList, paramName, paramComment, localizationService) {
        ValidateAllProperties();
    }

    [Required]
    public Element SelectedElem {
        get => _selectedElem;
        set => RaiseAndSetIfChanged(ref _selectedElem, value);
    }
}
