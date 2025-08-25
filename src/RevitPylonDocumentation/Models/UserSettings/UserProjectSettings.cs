using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitPylonDocumentation.Models.PylonSheetNView;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.UserSettings;
internal class UserProjectSettings : BaseViewModel {
    private string _projectSectionTemp = "обр_ФОП_Раздел проекта";
    private string _markTemp = "Марка";
    private string _titleBlockNameTemp = "Создать типы по комплектам";
    private string _dispatcherGroupingFirstTemp = "_Группа видов 1";
    private string _dispatcherGroupingSecondTemp = "_Группа видов 2";
    private string _sheetSizeTemp = "А";
    private string _sheetCoefficientTemp = "х";

    private string _sheetPrefixTemp = "Пилон ";
    private string _sheetSuffixTemp = "";

    private string _typicalPylonFilterParameterTemp = "обр_ФОП_Фильтрация 1";
    private string _typicalPylonFilterValueTemp = "на 1 шт.";

    private string _legendNameTemp = "Указания для пилонов корпуса";
    private string _legendXOffsetTemp = "-100";
    private string _legendYOffsetTemp = "130";

    private string _rebarNodeNameTemp = "КЖ_Узел ПК пилонов";
    private string _rebarNodeXOffsetTemp = "-150";
    private string _rebarNodeYOffsetTemp = "210";

    private string _pylonLengthParamNameTemp = "ФОП_РАЗМ_Длина";
    private string _pylonWidthParamNameTemp = "ФОП_РАЗМ_Ширина";

    private string _dimensionTypeNameTemp = "я_Основной_Плагин_2.5 мм";
    private string _spotDimensionTypeNameTemp = "Стрелка_Проектная_Верх";

    private string _skeletonTagTypeNameTemp = "Изделие_Марка - Полка 20";
    private string _rebarTagTypeWithSerifNameTemp = "Поз., Диаметр / Шаг - Полка 10, Засечка";
    private string _rebarTagTypeWithStepNameTemp = "Поз., Диаметр / Шаг - Полка 10";
    private string _rebarTagTypeWithCommentNameTemp = "Поз., Диаметр / Комментарий - Полка 10";
    private string _universalTagTypeNameTemp = "Без засечки";
    
    private string _breakLineTypeNameTemp = "Линейный обрыв";
    private string _concretingJointTypeNameTemp = "3 мм_М 20";

    public UserProjectSettings(MainViewModel mainViewModel, RevitRepository repository) {
        ViewModel = mainViewModel;
        Repository = repository;
    }

    public MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }


    public string ProjectSection { get; set; }
    public string ProjectSectionTemp {
        get => _projectSectionTemp;
        set => RaiseAndSetIfChanged(ref _projectSectionTemp, value);
    }

    public string Mark { get; set; }
    public string MarkTemp {
        get => _markTemp;
        set => RaiseAndSetIfChanged(ref _markTemp, value);
    }

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

    public string TypicalPylonFilterParameter { get; set; }
    public string TypicalPylonFilterParameterTemp {
        get => _typicalPylonFilterParameterTemp;
        set => RaiseAndSetIfChanged(ref _typicalPylonFilterParameterTemp, value);
    }

    public string TypicalPylonFilterValue { get; set; }
    public string TypicalPylonFilterValueTemp {
        get => _typicalPylonFilterValueTemp;
        set => RaiseAndSetIfChanged(ref _typicalPylonFilterValueTemp, value);
    }

    public string LegendName { get; set; }
    public string LegendNameTemp {
        get => _legendNameTemp;
        set => RaiseAndSetIfChanged(ref _legendNameTemp, value);
    }

    public string LegendXOffset { get; set; }
    public string LegendXOffsetTemp {
        get => _legendXOffsetTemp;
        set => RaiseAndSetIfChanged(ref _legendXOffsetTemp, value);
    }

    public string LegendYOffset { get; set; }
    public string LegendYOffsetTemp {
        get => _legendYOffsetTemp;
        set => RaiseAndSetIfChanged(ref _legendYOffsetTemp, value);
    }

    public string RebarNodeName { get; set; }
    public string RebarNodeNameTemp {
        get => _rebarNodeNameTemp;
        set => RaiseAndSetIfChanged(ref _rebarNodeNameTemp, value);
    }

    public string RebarNodeXOffset { get; set; }
    public string RebarNodeXOffsetTemp {
        get => _rebarNodeXOffsetTemp;
        set => RaiseAndSetIfChanged(ref _rebarNodeXOffsetTemp, value);
    }

    public string RebarNodeYOffset { get; set; }
    public string RebarNodeYOffsetTemp {
        get => _rebarNodeYOffsetTemp;
        set => RaiseAndSetIfChanged(ref _rebarNodeYOffsetTemp, value);
    }

    public string PylonLengthParamName { get; set; }
    public string PylonLengthParamNameTemp {
        get => _pylonLengthParamNameTemp;
        set => RaiseAndSetIfChanged(ref _pylonLengthParamNameTemp, value);
    }

    public string PylonWidthParamName { get; set; }
    public string PylonWidthParamNameTemp {
        get => _pylonWidthParamNameTemp;
        set => RaiseAndSetIfChanged(ref _pylonWidthParamNameTemp, value);
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
        ProjectSection = ProjectSectionTemp;
        Mark = MarkTemp;
        TitleBlockName = TitleBlockNameTemp;
        DispatcherGroupingFirst = DispatcherGroupingFirstTemp;
        DispatcherGroupingSecond = DispatcherGroupingSecondTemp;

        SheetSize = SheetSizeTemp;
        SheetCoefficient = SheetCoefficientTemp;
        SheetPrefix = SheetPrefixTemp;
        SheetSuffix = SheetSuffixTemp;

        TypicalPylonFilterParameter = TypicalPylonFilterParameterTemp;
        TypicalPylonFilterValue = TypicalPylonFilterValueTemp;

        LegendName = LegendNameTemp;
        LegendXOffset = LegendXOffsetTemp;
        LegendYOffset = LegendYOffsetTemp;

        RebarNodeName = RebarNodeNameTemp;
        RebarNodeXOffset = RebarNodeXOffsetTemp;
        RebarNodeYOffset = RebarNodeYOffsetTemp;

        PylonLengthParamName = PylonLengthParamNameTemp;
        PylonWidthParamName = PylonWidthParamNameTemp;

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
            ViewModel.ErrorText = "Наименование параметра диспетчера 1 некорректно";
        }
        if(Repository.AllSectionViews.FirstOrDefault()?.LookupParameter(DispatcherGroupingSecond) is null) {
            ViewModel.ErrorText = "Наименование параметра диспетчера 2 некорректно";
        }

        // Пытаемся проверить спеки
        if(Repository.AllScheduleViews.FirstOrDefault()?.LookupParameter(DispatcherGroupingFirst) is null) {
            ViewModel.ErrorText = "Наименование параметра диспетчера 1 некорректно";
        }
        if(Repository.AllScheduleViews.FirstOrDefault()?.LookupParameter(DispatcherGroupingSecond) is null) {
            ViewModel.ErrorText = "Наименование параметра диспетчера 2 некорректно";
        }

        // Проверяем, чтоб были заданы офсеты видового экрана легенды
        if(LegendXOffset is null || LegendYOffset is null) {
            ViewModel.ErrorText = "Не заданы отступы на листе для легенды примечений";
        }

        // Проверяем, чтоб были заданы офсеты видового экрана узла армирования
        if(RebarNodeXOffset is null || RebarNodeYOffset is null) {
            ViewModel.ErrorText = "Не заданы отступы на листе для узла армирования";
        }

        using(var transaction = Repository.Document.StartTransaction("Проверка параметров на листе")) {
            // Листов в проекте может не быть или рамка может быть другая, поэтому создаем свой лист для тестов с нужной рамкой
            var viewSheet = ViewSheet.Create(Repository.Document, ViewModel.SelectedTitleBlock.Id);
            if(viewSheet?.LookupParameter(DispatcherGroupingFirst) is null) {
                ViewModel.ErrorText = "Наименование параметра диспетчера 1 некорректно";
            }

            // Ищем рамку листа
            var titleBlock = new FilteredElementCollector(Repository.Document, viewSheet.Id)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsNotElementType()
                .FirstOrDefault() as FamilyInstance;

            if(titleBlock?.LookupParameter(SheetSize) is null) {
                ViewModel.ErrorText = "Наименование параметра формата рамки листа некорректно";
            }
            if(titleBlock?.LookupParameter(SheetCoefficient) is null) {
                ViewModel.ErrorText = "Наименование параметра множителя формата рамки листа некорректно";
            }

            // Удаляем созданный лист
            Repository.Document.Delete(viewSheet.Id);
            transaction.RollBack();
        }


        // Перебираем пилоны, которые найдены в проекте для работы и проверяем у НесКлн параметры сечения
        foreach(PylonSheetInfo sheetInfo in Repository.HostsInfo) {
            Element pylon = sheetInfo.HostElems.FirstOrDefault();
            if(pylon?.Category.GetBuiltInCategory() != BuiltInCategory.OST_StructuralColumns) { continue; }

            FamilySymbol pylonType = Repository.Document.GetElement(pylon?.GetTypeId()) as FamilySymbol;

            if(pylonType?.LookupParameter(PylonLengthParamName) is null) {
                ViewModel.ErrorText = "Наименование параметра длины сечения пилона некорректно";
                break;
            }

            if(pylonType?.LookupParameter(PylonWidthParamName) is null) {
                ViewModel.ErrorText = "Наименование параметра ширины сечения пилона некорректно";
                break;
            }
        }
    }
}
