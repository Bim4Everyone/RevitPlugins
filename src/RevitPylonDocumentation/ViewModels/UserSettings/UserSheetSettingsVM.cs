using System.ComponentModel.DataAnnotations;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitPylonDocumentation.Models;
using RevitPylonDocumentation.Models.UserSettings;

namespace RevitPylonDocumentation.ViewModels.UserSettings;
internal class UserSheetSettingsVM : ValidatableViewModel {
    private readonly ILocalizationService _localizationService;

    private string _sheetPrefixTemp = "Пилон ";
    private string _sheetSuffixTemp = "";
    private string _sheetSizeTemp = "А";
    private string _sheetCoefficientTemp = "х";
    private string _titleBlockNameTemp = "Создать типы по комплектам";
    private FamilySymbol _selectedTitleBlock;

    public UserSheetSettingsVM(MainViewModel mainViewModel, RevitRepository repository,
                               ILocalizationService localizationService) {
        ViewModel = mainViewModel;
        Repository = repository;
        _localizationService = localizationService;
        ValidateAllProperties();
    }

    public MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }

    public string SheetPrefix { get; set; }
    public string SheetPrefixTemp {
        get => _sheetPrefixTemp;
        set => RaiseAndSetIfChanged(ref _sheetPrefixTemp, value);
    }

    public string SheetSuffix { get; set; }
    public string SheetSuffixTemp {
        get => _sheetSuffixTemp;
        set => RaiseAndSetIfChanged(ref _sheetSuffixTemp, value);
    }

    public string SheetSize { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string SheetSizeTemp {
        get => _sheetSizeTemp;
        set {
            RaiseAndSetIfChanged(ref _sheetSizeTemp, value);
            ValidateProperty(value);
        }
    }

    public string SheetCoefficient { get; set; }
    [Required]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\]\{\};~]+$")]
    public string SheetCoefficientTemp {
        get => _sheetCoefficientTemp;
        set {
            RaiseAndSetIfChanged(ref _sheetCoefficientTemp, value);
            ValidateProperty(value);
        }
    }


    public string TitleBlockName { get; set; }
    public string TitleBlockNameTemp {
        get => _titleBlockNameTemp;
        set => RaiseAndSetIfChanged(ref _titleBlockNameTemp, value);
    }

    /// <summary>
    /// Выбранная пользователем рамка листа
    /// </summary>
    [Required]
    public FamilySymbol SelectedTitleBlock {
        get => _selectedTitleBlock;
        set {
            RaiseAndSetIfChanged(ref _selectedTitleBlock, value);
            TitleBlockNameTemp = value?.Name;
            ValidateProperty(value);
        }
    }

    public void ApplySheetSettings() {
        TitleBlockName = TitleBlockNameTemp;
        SheetSize = SheetSizeTemp;
        SheetCoefficient = SheetCoefficientTemp;
        SheetPrefix = SheetPrefixTemp;
        SheetSuffix = SheetSuffixTemp;
    }

    public void CheckSheetSettings() {
        using(var transaction = Repository.Document.StartTransaction("Checking parameters on sheet")) {
            // Листов в проекте может не быть или рамка может быть другая, поэтому создаем свой лист для тестов с нужной рамкой
            var viewSheet = ViewSheet.Create(Repository.Document, ViewModel.SheetSettings.SelectedTitleBlock.Id);
            if(viewSheet?.LookupParameter(ViewModel.ProjectSettings.DispatcherGroupingFirst) is null) {
                ViewModel.ErrorText = _localizationService.GetLocalizedString("VM.DispatcherGroupingFirstParamInvalid");
            }

            // Ищем рамку листа
            var titleBlock = new FilteredElementCollector(Repository.Document, viewSheet.Id)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsNotElementType()
                .FirstOrDefault() as FamilyInstance;

            if(titleBlock?.LookupParameter(SheetSize) is null) {
                ViewModel.ErrorText = _localizationService.GetLocalizedString("VM.SheetSizeParamInvalid");
            }
            if(titleBlock?.LookupParameter(SheetCoefficient) is null) {
                ViewModel.ErrorText = _localizationService.GetLocalizedString("VM.SheetCoefficientParamInvalid");
            }

            // Удаляем созданный лист
            Repository.Document.Delete(viewSheet.Id);
            transaction.RollBack();
        }
    }

    public UserSheetSettings GetSettings() {
        var settings = new UserSheetSettings();
        var vmType = this.GetType();
        var modelType = typeof(UserSheetSettings);

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
