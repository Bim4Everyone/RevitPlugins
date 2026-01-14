using System.ComponentModel.DataAnnotations;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitPylonDocumentation.Models;

namespace RevitPylonDocumentation.ViewModels.UserSettings;
internal class UserPylonSettingsVM : ValidatableViewModel {
    private readonly MainViewModel _viewModel;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string _projectSectionTemp = "обр_ФОП_Раздел проекта";
    private string _markTemp = "Марка";

    private string _typicalPylonFilterParameterTemp = "обр_ФОП_Фильтрация 1";
    private string _typicalPylonFilterValueTemp = "на 1 шт.";

    private string _pylonLengthParamNameTemp = "ФОП_РАЗМ_Длина";
    private string _pylonWidthParamNameTemp = "ФОП_РАЗМ_Ширина";

    public UserPylonSettingsVM(MainViewModel mainViewModel, RevitRepository repository,
                               ILocalizationService localizationService) {
        _viewModel = mainViewModel;
        _revitRepository = repository;
        _localizationService = localizationService;
        ValidateAllProperties();
    }

    public string ProjectSection { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string ProjectSectionTemp {
        get => _projectSectionTemp;
        set {
            RaiseAndSetIfChanged(ref _projectSectionTemp, value);
            ValidateProperty(value);
        }
    }

    public string Mark { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string MarkTemp {
        get => _markTemp;
        set {
            RaiseAndSetIfChanged(ref _markTemp, value);
            ValidateProperty(value);
        }
    }

    public string TypicalPylonFilterParameter { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string TypicalPylonFilterParameterTemp {
        get => _typicalPylonFilterParameterTemp;
        set {
            RaiseAndSetIfChanged(ref _typicalPylonFilterParameterTemp, value);
            ValidateProperty(value);
        }
    }

    public string TypicalPylonFilterValue { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string TypicalPylonFilterValueTemp {
        get => _typicalPylonFilterValueTemp;
        set {
            RaiseAndSetIfChanged(ref _typicalPylonFilterValueTemp, value);
            ValidateProperty(value);
        }
    }
    public string PylonLengthParamName { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string PylonLengthParamNameTemp {
        get => _pylonLengthParamNameTemp;
        set {
            RaiseAndSetIfChanged(ref _pylonLengthParamNameTemp, value);
            ValidateProperty(value);
        }
    }

    public string PylonWidthParamName { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string PylonWidthParamNameTemp {
        get => _pylonWidthParamNameTemp;
        set {
            RaiseAndSetIfChanged(ref _pylonWidthParamNameTemp, value);
            ValidateProperty(value);
        }
    }

    public bool CheckSettings() {
        // Перебираем пилоны, которые найдены в проекте для работы и проверяем у НесКлн параметры сечения
        foreach(var sheetInfo in _revitRepository.HostsInfo) {
            var pylon = sheetInfo.HostElems.FirstOrDefault();
            if(pylon?.Category.GetBuiltInCategory() != BuiltInCategory.OST_StructuralColumns) { continue; }

            var pylonType = _revitRepository.Document.GetElement(pylon?.GetTypeId()) as FamilySymbol;

            if(pylonType?.LookupParameter(PylonLengthParamName) is null) {
                _viewModel.ErrorText = _localizationService.GetLocalizedString("VM.PylonLengthParamInvalid");
                return false;
            }

            if(pylonType?.LookupParameter(PylonWidthParamName) is null) {
                _viewModel.ErrorText = _localizationService.GetLocalizedString("VM.PylonWidthParamInvalid");
                return false;
            }
        }
        return true;
    }
}
