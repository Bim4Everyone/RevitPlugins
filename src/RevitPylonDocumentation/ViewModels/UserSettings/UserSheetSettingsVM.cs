using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitPylonDocumentation.Models;

namespace RevitPylonDocumentation.ViewModels.UserSettings;
internal class UserSheetSettingsVM : ValidatableViewModel {
    private readonly MainViewModel _viewModel;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private string _sheetPrefixTemp = "Пилон ";
    private string _sheetSuffixTemp = "";
    private string _sheetSizeTemp = "А";
    private string _sheetCoefficientTemp = "х";
    private string _titleBlockNameTemp = "Создать типы по комплектам";
    private FamilySymbol _selectedTitleBlock;
    private bool _customTitleBlockIsCheck = false;
    private string _customSheetSizeValueTemp = "1";
    private string _customSheetCoefficientValueTemp = "1";

    public UserSheetSettingsVM(MainViewModel mainViewModel, RevitRepository repository,
                               ILocalizationService localizationService) {
        _viewModel = mainViewModel;
        _revitRepository = repository;
        _localizationService = localizationService;
        ValidateAllProperties();
    }

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

    public bool CustomTitleBlockIsCheck {
        get => _customTitleBlockIsCheck;
        set => RaiseAndSetIfChanged(ref _customTitleBlockIsCheck, value);
    }

    public string CustomSheetSizeValue { get; set; }
    [Required]
    [RegularExpression(@"^-?\d+$")]
    public string CustomSheetSizeValueTemp {
        get => _customSheetSizeValueTemp;
        set {
            RaiseAndSetIfChanged(ref _customSheetSizeValueTemp, value);
            ValidateProperty(value);
        }
    }

    public string CustomSheetCoefficientValue { get; set; }
    [Required]
    [RegularExpression(@"^-?\d+$")]
    public string CustomSheetCoefficientValueTemp {
        get => _customSheetCoefficientValueTemp;
        set {
            RaiseAndSetIfChanged(ref _customSheetCoefficientValueTemp, value);
            ValidateProperty(value);
        }
    }


    public bool CheckSettings() {
        if(SelectedTitleBlock is null) {
            SetError(_localizationService.GetLocalizedString("VM.SheetTypeNotSelected"));
            return false;
        }

        using(var transaction = _revitRepository.Document.StartTransaction("Checking parameters on sheet")) {
            // Листов в проекте может не быть или рамка может быть другая, поэтому создаем свой лист для тестов с нужной рамкой
            var viewSheet = ViewSheet.Create(_revitRepository.Document, _viewModel.SheetSettings.SelectedTitleBlock.Id);
            if(viewSheet?.LookupParameter(_viewModel.DispatcherSettings.DispatcherGroupingFirst) is null) {
                SetError(_localizationService.GetLocalizedString("VM.DispatcherGroupingFirstParamInvalid"));
            }

            // Ищем рамку листа
            var titleBlock = new FilteredElementCollector(_revitRepository.Document, viewSheet.Id)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsNotElementType()
                .FirstOrDefault() as FamilyInstance;

            bool hasError = false;
            if(titleBlock?.LookupParameter(SheetSize) is null) {
                SetError(_localizationService.GetLocalizedString("VM.SheetSizeParamInvalid"));
                hasError = true;
            }
            if(titleBlock?.LookupParameter(SheetCoefficient) is null) {
                SetError(_localizationService.GetLocalizedString("VM.SheetCoefficientParamInvalid"));
                hasError = true;
            }

            // Удаляем созданный лист
            _revitRepository.Document.Delete(viewSheet.Id);
            transaction.RollBack();

            if(hasError) { return false; }
        }

        if(!int.TryParse(CustomSheetSizeValue, out _)) {
            SetError(_localizationService.GetLocalizedString("VM.CustomSheetSizeValueInvalid"));
            return false;
        }
        if(!int.TryParse(CustomSheetCoefficientValue, out _)) {
            SetError(_localizationService.GetLocalizedString("VM.CustomSheetCoefficientValueInvalid"));
            return false;
        }
        return true;
    }

    /// <summary>
    /// Получает типоразмер рамки листа по имени типа
    /// </summary>
    public void FindTitleBlock() {
        if(!String.IsNullOrEmpty(TitleBlockName)) {
            SelectedTitleBlock = _viewModel.TitleBlocks
                .FirstOrDefault(titleBlock => titleBlock.Name.Contains(TitleBlockName));
        }
    }

    /// <summary>
    /// Записывает ошибку для отображения в GUI, указывая наименование вкладки, на которой произошла ошибка
    /// </summary>
    /// <param name="error"></param>
    private void SetError(string error) {
        _viewModel.ErrorText = string.Format(
            "{0} - {1}",
            _localizationService.GetLocalizedString("MainWindow.SheetParameters"),
            error);
    }
}
