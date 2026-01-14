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

    public bool CheckSettings() {
        if(SelectedTitleBlock is null) {
            _viewModel.ErrorText = _localizationService.GetLocalizedString("VM.SheetTypeNotSelected");
            return false;
        }

        using(var transaction = _revitRepository.Document.StartTransaction("Checking parameters on sheet")) {
            // Листов в проекте может не быть или рамка может быть другая, поэтому создаем свой лист для тестов с нужной рамкой
            var viewSheet = ViewSheet.Create(_revitRepository.Document, _viewModel.SheetSettings.SelectedTitleBlock.Id);
            if(viewSheet?.LookupParameter(_viewModel.ProjectSettings.DispatcherGroupingFirst) is null) {
                _viewModel.ErrorText = _localizationService.GetLocalizedString("VM.DispatcherGroupingFirstParamInvalid");
            }

            // Ищем рамку листа
            var titleBlock = new FilteredElementCollector(_revitRepository.Document, viewSheet.Id)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsNotElementType()
                .FirstOrDefault() as FamilyInstance;

            bool hasError = false;
            if(titleBlock?.LookupParameter(SheetSize) is null) {
                _viewModel.ErrorText = _localizationService.GetLocalizedString("VM.SheetSizeParamInvalid");
                hasError = true;
            }
            if(titleBlock?.LookupParameter(SheetCoefficient) is null) {
                _viewModel.ErrorText = _localizationService.GetLocalizedString("VM.SheetCoefficientParamInvalid");
                hasError = true;
            }

            // Удаляем созданный лист
            _revitRepository.Document.Delete(viewSheet.Id);
            transaction.RollBack();

            if(hasError) { return false; }
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
}
