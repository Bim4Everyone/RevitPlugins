using System.ComponentModel.DataAnnotations;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitPylonDocumentation.Models;
using RevitPylonDocumentation.Models.UserSettings;

namespace RevitPylonDocumentation.ViewModels.UserSettings;
internal class UserPylonSettingsVM : ValidatableViewModel {
    private readonly ILocalizationService _localizationService;

    private string _projectSectionTemp = "обр_ФОП_Раздел проекта";
    private string _markTemp = "Марка";

    private string _typicalPylonFilterParameterTemp = "обр_ФОП_Фильтрация 1";
    private string _typicalPylonFilterValueTemp = "на 1 шт.";

    private string _pylonLengthParamNameTemp = "ФОП_РАЗМ_Длина";
    private string _pylonWidthParamNameTemp = "ФОП_РАЗМ_Ширина";

    public UserPylonSettingsVM(MainViewModel mainViewModel, RevitRepository repository,
                               ILocalizationService localizationService) {
        ViewModel = mainViewModel;
        Repository = repository;
        _localizationService = localizationService;
        ValidateAllProperties();
    }

    public MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }


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

    public void ApplyScheduleFiltersSettings() {
        ProjectSection = ProjectSectionTemp;
        Mark = MarkTemp;

        TypicalPylonFilterParameter = TypicalPylonFilterParameterTemp;
        TypicalPylonFilterValue = TypicalPylonFilterValueTemp;

        PylonLengthParamName = PylonLengthParamNameTemp;
        PylonWidthParamName = PylonWidthParamNameTemp;
    }

    public void CheckProjectSettings() {
        // Перебираем пилоны, которые найдены в проекте для работы и проверяем у НесКлн параметры сечения
        foreach(var sheetInfo in Repository.HostsInfo) {
            var pylon = sheetInfo.HostElems.FirstOrDefault();
            if(pylon?.Category.GetBuiltInCategory() != BuiltInCategory.OST_StructuralColumns) { continue; }

            var pylonType = Repository.Document.GetElement(pylon?.GetTypeId()) as FamilySymbol;

            if(pylonType?.LookupParameter(PylonLengthParamName) is null) {
                ViewModel.ErrorText = _localizationService.GetLocalizedString("VM.PylonLengthParamInvalid");
                break;
            }

            if(pylonType?.LookupParameter(PylonWidthParamName) is null) {
                ViewModel.ErrorText = _localizationService.GetLocalizedString("VM.PylonWidthParamInvalid");
                break;
            }
        }
    }

    public UserPylonSettings GetSettings() {
        var settings = new UserPylonSettings();
        var vmType = this.GetType();
        var modelType = typeof(UserPylonSettings);

        foreach(var prop in modelType.GetProperties()) {
            var vmProp = vmType.GetProperty(prop.Name);
            if(vmProp != null && vmProp.CanRead && prop.CanWrite) {
                var value = vmProp.GetValue(this);
                prop.SetValue(settings, value);
            }
        }
        return settings;
    }
}
