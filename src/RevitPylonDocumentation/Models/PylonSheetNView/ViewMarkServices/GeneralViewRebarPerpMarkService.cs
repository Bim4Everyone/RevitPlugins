using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices;
internal class GeneralViewRebarPerpMarkService {
    private readonly ViewPointsAnalyzer _viewPointsAnalyzer;
    private readonly AnnotationService _annotationService;
    private readonly FamilySymbol _tagSymbolWithoutSerif;

    internal GeneralViewRebarPerpMarkService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo,
                                PylonView pylonView) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        ViewOfPylon = pylonView;

        _viewPointsAnalyzer = new ViewPointsAnalyzer(ViewOfPylon);
        _annotationService = new AnnotationService(ViewOfPylon);

        ////// Находим типоразмер марки несущей арматуры для обозначения марки изделия
        ////_tagSkeletonSymbol = Repository.FindSymbol(BuiltInCategory.OST_RebarTags, "Изделие_Марка - Полка 30");

        // Находим типоразмер марки несущей арматуры для обозначения позиции, диаметра и комментариев арматуры
        // Без засечки на конце
        _tagSymbolWithoutSerif = Repository.FindSymbol(BuiltInCategory.OST_RebarTags, "Поз., Диаметр / Шаг - Полка 10");
        ////// Находим типоразмер типовой аннотации для метки ГОСТа сварки
        ////_gostTagSymbol = Repository.FindSymbol(BuiltInCategory.OST_GenericAnnotation, "Без засечки");
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }
    internal PylonView ViewOfPylon { get; set; }


    internal void CreateVerticalBarMarks() {
        var simpleRebars = SheetInfo.RebarInfo.SimpleVerticalRebars;
        if(simpleRebars.Count == 0) { return; }
        var simpleRebarsInView = ViewModel.RebarFinder.GetRebarsFromView(simpleRebars, ViewOfPylon.ViewElement);
        if(simpleRebarsInView.Count == 0) { return; }

        CreateVerticalBarMark(simpleRebarsInView, DirectionType.RightTop);
        CreateVerticalBarMark(simpleRebarsInView, DirectionType.LeftTop);
    }

    private void CreateVerticalBarMark(List<Element> rebars, DirectionType directionType) {
        // Получаем референс-элемент
        Element verticalBar = _viewPointsAnalyzer.GetElementByDirection(rebars, directionType, false);
        // Получаем точку в которую нужно поставить аннотацию
        var point = _viewPointsAnalyzer.GetPointByDirection(verticalBar, directionType, 0, 0, true);
        // Корректируем положение точки, куда будет установлена марка (текст)
        point = _viewPointsAnalyzer.GetPointByDirection(point, directionType, 0.6, 0.3);
        // Создаем марку арматуры
        var tag = _annotationService.CreateRebarTag(point, _tagSymbolWithoutSerif, verticalBar);
        tag.LeaderEndCondition = LeaderEndCondition.Free;
    }
}
