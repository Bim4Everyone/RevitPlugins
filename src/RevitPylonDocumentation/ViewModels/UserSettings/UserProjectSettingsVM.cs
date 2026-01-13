using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitPylonDocumentation.Models;
using RevitPylonDocumentation.Models.UserSettings;

namespace RevitPylonDocumentation.ViewModels.UserSettings;
internal class UserProjectSettingsVM : BaseViewModel {
    private readonly ILocalizationService _localizationService;

    private string _titleBlockNameTemp = "Создать типы по комплектам";
    private string _dispatcherGroupingFirstTemp = "_Группа видов 1";
    private string _dispatcherGroupingSecondTemp = "_Группа видов 2";
    private string _sheetSizeTemp = "А";
    private string _sheetCoefficientTemp = "х";

    private string _sheetPrefixTemp = "Пилон ";
    private string _sheetSuffixTemp = "";

    private string _dimensionTypeNameTemp = "я_Основной_Плагин_2.5 мм";
    private string _spotDimensionTypeNameTemp = "Стрелка_Проектная_Верх";

    private string _skeletonTagTypeNameTemp = "Изделие_Марка - Полка 7";
    private string _rebarTagTypeWithSerifNameTemp = "Поз., Диаметр / Шаг - Полка 10, Засечка";
    private string _rebarTagTypeWithStepNameTemp = "Поз., Диаметр / Шаг - Полка 10";
    private string _rebarTagTypeWithCommentNameTemp = "Поз., Диаметр / Комментарий - Полка 10";
    private string _universalTagTypeNameTemp = "Без засечки";

    private string _breakLineTypeNameTemp = "Линейный обрыв";
    private string _concretingJointTypeNameTemp = "3 мм_М 20";

    public UserProjectSettingsVM(MainViewModel mainViewModel, RevitRepository repository,
                                 ILocalizationService localizationService) {
        ViewModel = mainViewModel;
        Repository = repository;
        _localizationService = localizationService;
    }

    public MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }



    public string TitleBlockName { get; set; }
    public string TitleBlockNameTemp {
        get => _titleBlockNameTemp;
        set => RaiseAndSetIfChanged(ref _titleBlockNameTemp, value);
    }

    public string DispatcherGroupingFirst { get; set; }
    public string DispatcherGroupingFirstTemp {
        get => _dispatcherGroupingFirstTemp;
        set => RaiseAndSetIfChanged(ref _dispatcherGroupingFirstTemp, value);
    }
    public string DispatcherGroupingSecond { get; set; }
    public string DispatcherGroupingSecondTemp {
        get => _dispatcherGroupingSecondTemp;
        set => RaiseAndSetIfChanged(ref _dispatcherGroupingSecondTemp, value);
    }

    public string SheetSize { get; set; }
    public string SheetSizeTemp {
        get => _sheetSizeTemp;
        set => RaiseAndSetIfChanged(ref _sheetSizeTemp, value);
    }

    public string SheetCoefficient { get; set; }
    public string SheetCoefficientTemp {
        get => _sheetCoefficientTemp;
        set => RaiseAndSetIfChanged(ref _sheetCoefficientTemp, value);
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


    public string DimensionTypeName { get; set; }
    public string DimensionTypeNameTemp {
        get => _dimensionTypeNameTemp;
        set => RaiseAndSetIfChanged(ref _dimensionTypeNameTemp, value);
    }

    public string SpotDimensionTypeName { get; set; }
    public string SpotDimensionTypeNameTemp {
        get => _spotDimensionTypeNameTemp;
        set => RaiseAndSetIfChanged(ref _spotDimensionTypeNameTemp, value);
    }

    public string SkeletonTagTypeName { get; set; }
    public string SkeletonTagTypeNameTemp {
        get => _skeletonTagTypeNameTemp;
        set => RaiseAndSetIfChanged(ref _skeletonTagTypeNameTemp, value);
    }

    public string RebarTagTypeWithSerifName { get; set; }
    public string RebarTagTypeWithSerifNameTemp {
        get => _rebarTagTypeWithSerifNameTemp;
        set => RaiseAndSetIfChanged(ref _rebarTagTypeWithSerifNameTemp, value);
    }

    public string RebarTagTypeWithStepName { get; set; }
    public string RebarTagTypeWithStepNameTemp {
        get => _rebarTagTypeWithStepNameTemp;
        set => RaiseAndSetIfChanged(ref _rebarTagTypeWithStepNameTemp, value);
    }

    public string RebarTagTypeWithCommentName { get; set; }
    public string RebarTagTypeWithCommentNameTemp {
        get => _rebarTagTypeWithCommentNameTemp;
        set => RaiseAndSetIfChanged(ref _rebarTagTypeWithCommentNameTemp, value);
    }

    public string UniversalTagTypeName { get; set; }
    public string UniversalTagTypeNameTemp {
        get => _universalTagTypeNameTemp;
        set => RaiseAndSetIfChanged(ref _universalTagTypeNameTemp, value);
    }

    public string BreakLineTypeName { get; set; }
    public string BreakLineTypeNameTemp {
        get => _breakLineTypeNameTemp;
        set => RaiseAndSetIfChanged(ref _breakLineTypeNameTemp, value);
    }

    public string ConcretingJointTypeName { get; set; }
    public string ConcretingJointTypeNameTemp {
        get => _concretingJointTypeNameTemp;
        set => RaiseAndSetIfChanged(ref _concretingJointTypeNameTemp, value);
    }

    public void ApplyProjectSettings() {

        TitleBlockName = TitleBlockNameTemp;
        DispatcherGroupingFirst = DispatcherGroupingFirstTemp;
        DispatcherGroupingSecond = DispatcherGroupingSecondTemp;

        SheetSize = SheetSizeTemp;
        SheetCoefficient = SheetCoefficientTemp;
        SheetPrefix = SheetPrefixTemp;
        SheetSuffix = SheetSuffixTemp;


        DimensionTypeName = DimensionTypeNameTemp;
        SpotDimensionTypeName = SpotDimensionTypeNameTemp;

        SkeletonTagTypeName = SkeletonTagTypeNameTemp;
        RebarTagTypeWithSerifName = RebarTagTypeWithSerifNameTemp;
        RebarTagTypeWithStepName = RebarTagTypeWithStepNameTemp;
        RebarTagTypeWithCommentName = RebarTagTypeWithCommentNameTemp;
        UniversalTagTypeName = UniversalTagTypeNameTemp;
        BreakLineTypeName = BreakLineTypeNameTemp;
        ConcretingJointTypeName = ConcretingJointTypeNameTemp;
    }

    public void CheckProjectSettings() {
        // Пытаемся проверить виды
        if(Repository.AllSectionViews.FirstOrDefault()?.LookupParameter(DispatcherGroupingFirst) is null) {
            ViewModel.ErrorText = _localizationService.GetLocalizedString("VM.DispatcherGroupingFirstParamInvalid");
        }
        if(Repository.AllSectionViews.FirstOrDefault()?.LookupParameter(DispatcherGroupingSecond) is null) {
            ViewModel.ErrorText = _localizationService.GetLocalizedString("VM.DispatcherGroupingSecondParamInvalid");
        }

        // Пытаемся проверить спеки
        if(Repository.AllScheduleViews.FirstOrDefault()?.LookupParameter(DispatcherGroupingFirst) is null) {
            ViewModel.ErrorText = _localizationService.GetLocalizedString("VM.DispatcherGroupingFirstParamInvalid");
        }
        if(Repository.AllScheduleViews.FirstOrDefault()?.LookupParameter(DispatcherGroupingSecond) is null) {
            ViewModel.ErrorText = _localizationService.GetLocalizedString("VM.DispatcherGroupingSecondParamInvalid");
        }

        using(var transaction = Repository.Document.StartTransaction("Checking parameters on sheet")) {
            // Листов в проекте может не быть или рамка может быть другая, поэтому создаем свой лист для тестов с нужной рамкой
            var viewSheet = ViewSheet.Create(Repository.Document, ViewModel.TypesSettings.SelectedTitleBlock.Id);
            if(viewSheet?.LookupParameter(DispatcherGroupingFirst) is null) {
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

    public UserProjectSettings GetSettings() {
        var settings = new UserProjectSettings();
        var vmType = this.GetType();
        var modelType = typeof(UserProjectSettings);

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
