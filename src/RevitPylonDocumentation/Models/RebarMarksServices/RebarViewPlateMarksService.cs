using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PylonSheetNView;

namespace RevitPylonDocumentation.Models.RebarMarksServices;
internal class RebarViewPlateMarksService {
    private readonly ViewPointsAnalyzer _viewPointsAnalyzer;
    private readonly AnnotationService _annotationService;
    private readonly FamilySymbol _tagSymbolWithoutSerif;

    public RebarViewPlateMarksService(PylonView pylonView, RevitRepository revitRepository) {
        _viewPointsAnalyzer = new ViewPointsAnalyzer(pylonView);
        _annotationService = new AnnotationService(pylonView);

        // Находим типоразмер марки несущей арматуры для обозначения позиции, диаметра и комментариев арматуры
        // Без засечки на конце
        _tagSymbolWithoutSerif = revitRepository.FindSymbol(BuiltInCategory.OST_RebarTags, "Поз., Диаметр / Комментарий - Полка 10");
    }



    public void CreateTopMark(List<Element> simplePlates) {

        // Получаем референс-элемент
        Element topPlate = _viewPointsAnalyzer.GetElementByDirection(simplePlates, DirectionType.Top, true);

        // Получаем точку в которую нужно поставить аннотацию
        var pointTopPlateLeader = _viewPointsAnalyzer.GetPointByDirection(topPlate, DirectionType.Right, 0.2, 0, true);
        var pointTopPlate = _viewPointsAnalyzer.GetPointByDirection(topPlate, DirectionType.LeftBottom, 0.5, 0.4, true);

        // Создаем марку арматуры
        var topPlateTag = _annotationService.CreateRebarTag(pointTopPlate, _tagSymbolWithoutSerif, topPlate);
        topPlateTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
        topPlateTag.SetLeaderEnd(new Reference(topPlate), pointTopPlateLeader);
#endif
    }


    public void CreateBottomMark(List<Element> simplePlates) {
        // Получаем референс-элемент
        Element bottomPlate = _viewPointsAnalyzer.GetElementByDirection(simplePlates, DirectionType.Bottom, true);

        // Получаем точку в которую нужно поставить аннотацию
        var pointBottomPlateLeader = _viewPointsAnalyzer.GetPointByDirection(bottomPlate, DirectionType.Left, 0.3, 0, true);
        var pointBottomPlate = _viewPointsAnalyzer.GetPointByDirection(bottomPlate, DirectionType.RightTop, 0.35, 0.3, true);

        // Создаем марку арматуры
        var bottomPlateTag = _annotationService.CreateRebarTag(pointBottomPlate, _tagSymbolWithoutSerif, bottomPlate);
        bottomPlateTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
        bottomPlateTag.SetLeaderEnd(new Reference(bottomPlate), pointBottomPlateLeader);
#endif
    }


    public void CreateLeftMark(List<Element> simplePlates) {
        // Получаем референс-элемент
        Element leftPlate = _viewPointsAnalyzer.GetElementByDirection(simplePlates, DirectionType.Left, true);

        // Получаем точку в которую нужно поставить аннотацию
        var pointLeftPlateLeader = _viewPointsAnalyzer.GetPointByDirection(leftPlate, DirectionType.Bottom, 0.4, 0, true);
        var pointLeftPlate = _viewPointsAnalyzer.GetPointByDirection(leftPlate, DirectionType.LeftBottom, 0.8, 0.3, true);

        // Создаем марку арматуры
        var leftPlateTag = _annotationService.CreateRebarTag(pointLeftPlate, _tagSymbolWithoutSerif, leftPlate);
        leftPlateTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
        leftPlateTag.SetLeaderEnd(new Reference(leftPlate), pointLeftPlateLeader);
#endif
    }



    public void CreateRightMark(List<Element> simplePlates) {
        // Получаем референс-элемент
        Element rightPlate = _viewPointsAnalyzer.GetElementByDirection(simplePlates, DirectionType.Right, true);

        // Получаем точку в которую нужно поставить аннотацию
        var pointRightPlateLeader = _viewPointsAnalyzer.GetPointByDirection(rightPlate, DirectionType.Top, 0.4, 0, true);
        var pointRightPlate = _viewPointsAnalyzer.GetPointByDirection(rightPlate, DirectionType.RightTop, 0.8, 0.6, true);

        // Создаем марку арматуры
        var rightPlateTag = _annotationService.CreateRebarTag(pointRightPlate, _tagSymbolWithoutSerif, rightPlate);
        rightPlateTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
        rightPlateTag.SetLeaderEnd(new Reference(rightPlate), pointRightPlateLeader);
#endif
    }
}
