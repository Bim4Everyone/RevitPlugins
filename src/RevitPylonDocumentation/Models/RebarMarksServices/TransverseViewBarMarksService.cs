using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PylonSheetNView;

namespace RevitPylonDocumentation.Models.RebarMarksServices;
internal class TransverseViewBarMarksService {
    private readonly PylonView _pylonView;
    private readonly ViewPointsAnalyzer _viewPointsAnalyzer;
    private readonly AnnotationService _annotationService;
    private readonly FamilySymbol _tagSymbolWithSerif;
    private readonly FamilySymbol _tagSymbolWithoutSerif;
    private readonly FamilySymbol _tagSkeletonSymbol;

    public TransverseViewBarMarksService(PylonView pylonView, RevitRepository revitRepository) {
        _pylonView = pylonView;
        _viewPointsAnalyzer = new ViewPointsAnalyzer(pylonView);
        _annotationService = new AnnotationService(pylonView);

        // Находим типоразмер марки несущей арматуры для обозначения позиции, диаметра и комментариев арматуры
        // Без засечки на конце
        _tagSymbolWithoutSerif = revitRepository.FindSymbol(BuiltInCategory.OST_RebarTags, "Поз., Диаметр / Шаг - Полка 10");
        // С засечкой на конце
        _tagSymbolWithSerif = revitRepository.FindSymbol(BuiltInCategory.OST_RebarTags, "Поз., Диаметр / Шаг - Полка 10, Засечка");

        // Находим типоразмер марки несущей арматуры для обозначения марки изделия
        _tagSkeletonSymbol = revitRepository.FindSymbol(BuiltInCategory.OST_RebarTags, "Изделие_Марка - Полка 30");
    }


    public void CreateLeftBottomMark(List<Element> simpleRebars) {
        // Получаем референс-элемент
        Element leftBottomVerticalBar = _viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.LeftBottom, false);

        // Получаем точку в которую нужно поставить аннотацию
        var pointLeftBottom = _viewPointsAnalyzer.GetPointByDirection(leftBottomVerticalBar, DirectionType.LeftBottom, 1.5, 0.6, false);


        // Создаем марку арматуры
        var leftBottomTag = _annotationService.CreateRebarTag(pointLeftBottom, _tagSkeletonSymbol, leftBottomVerticalBar);
    }


    public void CreateLeftTopMark(List<Element> simpleClamps, List<Element> simpleRebars) {
        // Получаем референс-элемент
        Element leftClamp = _viewPointsAnalyzer.GetElementByDirection(simpleClamps, DirectionType.Left, false);
        Element leftTopVerticalBar = _viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.LeftTop, false);

        // Получаем точку в которую нужно поставить аннотацию
        var pointLeftTop = _viewPointsAnalyzer.GetPointByDirection(leftTopVerticalBar, DirectionType.LeftTop, 1.2, 0.6, false);

        // Создаем марку арматуры
        var leftTopTag = _annotationService.CreateRebarTag(pointLeftTop, _tagSymbolWithoutSerif, leftClamp);

#if REVIT_2022_OR_GREATER
        leftTopTag.LeaderEndCondition = LeaderEndCondition.Free;
        var leftTopClampRef = new Reference(leftClamp);

        var tagLeaderEnd = leftTopTag.GetLeaderEnd(leftTopClampRef);
        tagLeaderEnd = _viewPointsAnalyzer.GetPointByDirection(tagLeaderEnd, DirectionType.Top, 0, 0.2);
        leftTopTag.SetLeaderEnd(leftTopClampRef, tagLeaderEnd);
#endif
    }


    public void CreateRightTopMark(List<Element> simpleClamps, List<Element> simpleRebars) {
        // Получаем референс-элемент
        Element rightClamp = _viewPointsAnalyzer.GetElementByDirection(simpleClamps, DirectionType.Right, true);
        Element rightTopVerticalBar = _viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.RightTop, false);

        // Получаем точку в которую нужно поставить аннотацию
        var pointRightTop = _viewPointsAnalyzer.GetPointByDirection(rightTopVerticalBar, DirectionType.RightTop, 1.2, 0.6, false);

        // Создаем марку арматуры
        var rightTopTag = _annotationService.CreateRebarTag(pointRightTop, _tagSymbolWithoutSerif, rightClamp);

#if REVIT_2022_OR_GREATER
        rightTopTag.LeaderEndCondition = LeaderEndCondition.Free;
        var rightTopClampRef = new Reference(rightClamp);

        var tagLeaderEnd = rightTopTag.GetLeaderEnd(rightTopClampRef);
        tagLeaderEnd = _viewPointsAnalyzer.GetPointByDirection(tagLeaderEnd, DirectionType.Top, 0, 0.2);
        rightTopTag.SetLeaderEnd(rightTopClampRef, tagLeaderEnd);
#endif
    }


    public void CreateLeftMark(List<Element> simpleCBars, List<Element> simpleRebars) {
        // Получаем референс-элемент
        Element leftCBar = _viewPointsAnalyzer.GetElementByDirection(simpleCBars, DirectionType.Left, true);

        // Получаем точку в которую нужно поставить аннотацию
        var pointLeft = _viewPointsAnalyzer.GetPointByDirection(leftCBar, DirectionType.LeftBottom, 1.3, 0.1, true);

        // Создаем марку арматуры
        var leftTag = _annotationService.CreateRebarTag(pointLeft, _tagSymbolWithSerif, simpleCBars);
        leftTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
        foreach(var simpleCBar in simpleCBars) {
            var simpleCBarRef = new Reference(simpleCBar);
            var tagLeaderEndPoint = _viewPointsAnalyzer.GetPointByDirection(simpleCBar, DirectionType.LeftBottom, 0, 0.1, true);
            leftTag.SetLeaderEnd(simpleCBarRef, tagLeaderEndPoint);
        }
#endif
    }
}
