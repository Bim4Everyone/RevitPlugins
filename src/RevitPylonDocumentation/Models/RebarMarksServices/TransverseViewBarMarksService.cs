using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.Models.PylonSheetNView;

namespace RevitPylonDocumentation.Models.RebarMarksServices {
    internal class TransverseViewBarMarksService {
        private readonly string _commentParamName = "Комментарии";

        private readonly PylonView _pylonView;
        private readonly ViewPointsAnalyzer _viewPointsAnalyzer;
        private readonly AnnotationService _annotationService;
        private readonly FamilySymbol _tagSymbol;
        private readonly FamilySymbol _gostTagSymbol;

        public TransverseViewBarMarksService(PylonView pylonView, RevitRepository revitRepository) {
            _pylonView = pylonView;
            _viewPointsAnalyzer = new ViewPointsAnalyzer(pylonView);
            _annotationService = new AnnotationService(pylonView);

            // Находим типоразмер марки несущей арматуры
            _tagSymbol = revitRepository.FindSymbol(BuiltInCategory.OST_RebarTags, "Поз., Диаметр / Комментарий - Полка 10");
            // Находим типоразмер типовой аннотации для метки ГОСТа сварки
            _gostTagSymbol = revitRepository.FindSymbol(BuiltInCategory.OST_GenericAnnotation, "Без засечки");
        }


        public void CreateLeftTopMarks(List<Element> simpleClamps, List<Element> simpleRebars) {
            // Получаем референс-элемент
            Element leftTopClamp = _viewPointsAnalyzer.GetElementByDirection(simpleClamps, DirectionType.Left, false);
            Element leftTopVerticalBar = _viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.LeftTop, false);

            // Устанавливаем значение комментария у арматуры, к которой привяжем марку
            leftTopClamp.SetParamValue(_commentParamName, "шаг 100");

            // Получаем точку в которую нужно поставить аннотацию
            XYZ pointLeftTop = _viewPointsAnalyzer.GetPointByDirection(leftTopVerticalBar, DirectionType.LeftTop, 1, 0.5, false);

            // Создаем марку арматуры
            var leftTopTag = _annotationService.CreateRebarTag(pointLeftTop, _tagSymbol, leftTopClamp);

#if REVIT_2022_OR_GREATER
            leftTopTag.LeaderEndCondition = LeaderEndCondition.Free;
            var leftTopClampRef = new Reference(leftTopClamp);

            var tagLeaderEnd = leftTopTag.GetLeaderEnd(leftTopClampRef);
            tagLeaderEnd = _viewPointsAnalyzer.GetPointByDirection(tagLeaderEnd, DirectionType.Top, 0, 0.1);
            leftTopTag.SetLeaderEnd(leftTopClampRef, tagLeaderEnd);
#endif
        }
    }
}
