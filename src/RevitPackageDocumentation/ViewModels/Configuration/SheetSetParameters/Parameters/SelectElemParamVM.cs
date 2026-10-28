using System.ComponentModel.DataAnnotations;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;

/// <summary>
/// Параметр комплекта "Выбор элемента": элемент модели, относительно которого модули видов создают виды
/// (уровень плана, положение разрезов и фрагментов). Выбранный элемент в конфигурации не сохраняется
/// </summary>
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
