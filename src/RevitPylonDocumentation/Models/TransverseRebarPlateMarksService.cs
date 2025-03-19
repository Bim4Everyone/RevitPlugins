using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PylonSheetNView;

namespace RevitPylonDocumentation.Models {
    internal class TransverseRebarPlateMarksService {
        private readonly ViewPointsAnalyzer _viewPointsAnalyzer;
        private readonly AnnotationService _annotationService;
        private readonly FamilySymbol _tagSymbol;

        public TransverseRebarPlateMarksService(PylonView pylonView, RevitRepository revitRepository) {
            _viewPointsAnalyzer = new ViewPointsAnalyzer(pylonView);
            _annotationService = new AnnotationService(pylonView);

            // Находим типоразмер марки несущей арматуры
            _tagSymbol = revitRepository.FindSymbol(BuiltInCategory.OST_RebarTags, "Поз., Диаметр / Комментарий - Полка 10");
        }



        public void CreateTransversePlateTopMarks(List<Element> simplePlates) {

            // Получаем референс-элемент
            Element topPlate = _viewPointsAnalyzer.GetElementByDirection(simplePlates, DirectionType.Top, true);

            // Получаем точку в которую нужно поставить аннотацию
            XYZ pointTopPlateLeader = _viewPointsAnalyzer.GetPointByDirection(topPlate, DirectionType.Right, 0.2, 0, true);
            XYZ pointTopPlate = _viewPointsAnalyzer.GetPointByDirection(topPlate, DirectionType.LeftBottom, 0.5, 0.4, true);

            // Создаем марку арматуры
            var topPlateTag = _annotationService.CreateRebarTag(pointTopPlate, _tagSymbol, topPlate);
            topPlateTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
            topPlateTag.SetLeaderEnd(new Reference(topPlate), pointTopPlateLeader);
#endif
        }


        public void CreateTransversePlateBottomMarks(List<Element> simplePlates) {
            // Получаем референс-элемент
            Element bottomPlate = _viewPointsAnalyzer.GetElementByDirection(simplePlates, DirectionType.Bottom, true);

            // Получаем точку в которую нужно поставить аннотацию
            XYZ pointBottomPlateLeader = _viewPointsAnalyzer.GetPointByDirection(bottomPlate, DirectionType.Left, 0.2, 0, true);
            XYZ pointBottomPlate = _viewPointsAnalyzer.GetPointByDirection(bottomPlate, DirectionType.RightTop, 0.45, 0.3, true);

            // Создаем марку арматуры
            var bottomPlateTag = _annotationService.CreateRebarTag(pointBottomPlate, _tagSymbol, bottomPlate);
            bottomPlateTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
            bottomPlateTag.SetLeaderEnd(new Reference(bottomPlate), pointBottomPlateLeader);
#endif
        }


        public void CreateTransversePlateLeftMarks(List<Element> simplePlates) {
            // Получаем референс-элемент
            Element leftPlate = _viewPointsAnalyzer.GetElementByDirection(simplePlates, DirectionType.Left, true);

            // Получаем точку в которую нужно поставить аннотацию
            XYZ pointLeftPlateLeader = _viewPointsAnalyzer.GetPointByDirection(leftPlate, DirectionType.Bottom, 0.4, 0, true);
            XYZ pointLeftPlate = _viewPointsAnalyzer.GetPointByDirection(leftPlate, DirectionType.LeftBottom, 0.8, 0.3, true);

            // Создаем марку арматуры
            var leftPlateTag = _annotationService.CreateRebarTag(pointLeftPlate, _tagSymbol, leftPlate);
            leftPlateTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
            leftPlateTag.SetLeaderEnd(new Reference(leftPlate), pointLeftPlateLeader);
#endif
        }



        public void CreateTransversePlateRightMarks(List<Element> simplePlates) {
            // Получаем референс-элемент
            Element rightPlate = _viewPointsAnalyzer.GetElementByDirection(simplePlates, DirectionType.Right, true);

            // Получаем точку в которую нужно поставить аннотацию
            XYZ pointRightPlateLeader = _viewPointsAnalyzer.GetPointByDirection(rightPlate, DirectionType.Top, 0.4, 0, true);
            XYZ pointRightPlate = _viewPointsAnalyzer.GetPointByDirection(rightPlate, DirectionType.RightTop, 0.8, 0.6, true);

            // Создаем марку арматуры
            var rightPlateTag = _annotationService.CreateRebarTag(pointRightPlate, _tagSymbol, rightPlate);
            rightPlateTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
            rightPlateTag.SetLeaderEnd(new Reference(rightPlate), pointRightPlateLeader);
#endif
        }
    }
}
