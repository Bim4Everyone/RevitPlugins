using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.Models.PylonSheetNView;

namespace RevitPylonDocumentation.Models.RebarMarksServices;
internal class RebarViewBarMarksService {
    private readonly string _commentParamName = "Комментарии";

    private readonly ViewPointsAnalyzer _viewPointsAnalyzer;
    private readonly AnnotationService _annotationService;
    private readonly FamilySymbol _tagSymbolWithoutSerif;
    private readonly FamilySymbol _gostTagSymbol;

    public RebarViewBarMarksService(PylonView pylonView, RevitRepository revitRepository) {
        _viewPointsAnalyzer = new ViewPointsAnalyzer(pylonView);
        _annotationService = new AnnotationService(pylonView);

        // Находим типоразмер марки несущей арматуры для обозначения позиции, диаметра и комментариев арматуры
        // Без засечки на конце
        _tagSymbolWithoutSerif = revitRepository.FindSymbol(BuiltInCategory.OST_RebarTags, "Поз., Диаметр / Комментарий - Полка 10");
        // Находим типоразмер типовой аннотации для метки ГОСТа сварки
        _gostTagSymbol = revitRepository.FindSymbol(BuiltInCategory.OST_GenericAnnotation, "Без засечки");
    }



    public void CreateLeftBottomMark(List<Element> simpleRebars, bool hasLRebar) {
        // Получаем референс-элемент
        var leftBottomElement = _viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.LeftBottom, false);

        // Устанавливаем значение комментария у арматуры, к которой привяжем марку
        string commentValue = hasLRebar ? $"{simpleRebars.Count / 2} шт." : $"{simpleRebars.Count} шт.";
        leftBottomElement.SetParamValue(_commentParamName, commentValue);

        // Получаем точку в которую нужно поставить аннотацию
        var pointLeftBottom = _viewPointsAnalyzer.GetPointByDirection(leftBottomElement, DirectionType.LeftBottom, 1, 0.4, false);

        // Создаем марку арматуры
        _annotationService.CreateRebarTag(pointLeftBottom, _tagSymbolWithoutSerif, leftBottomElement);
    }


    public void CreateLeftTopMark(List<Element> simpleRebars) {
        // Получаем референс-элемент
        var leftTopElement = _viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.LeftTop, false);

        // Устанавливаем значение комментария у арматуры, к которой привяжем марку
        leftTopElement.SetParamValue(_commentParamName, $"{simpleRebars.Count / 2} шт.");

        // Получаем точку в которую нужно поставить аннотацию
        var pointLeftTop = _viewPointsAnalyzer.GetPointByDirection(leftTopElement, DirectionType.LeftTop, 1, 0.4, false);

        // Создаем марку арматуры
        _annotationService.CreateRebarTag(pointLeftTop, _tagSymbolWithoutSerif, leftTopElement);
    }


    public void CreateRightBottomMark(List<Element> simpleRebars) {
        // Получаем референс-элемент
        var rightBottomElement = _viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.RightBottom, false);

        // Получаем точку в которую нужно поставить аннотацию
        var pointRightBottom = _viewPointsAnalyzer.GetPointByDirection(rightBottomElement, DirectionType.RightBottom, 2, 0.4, false);

        // Создаем типовую аннотацию для обозначения ГОСТа
        _annotationService.CreateGostTag(pointRightBottom, _gostTagSymbol, rightBottomElement);
    }
}
