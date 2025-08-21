using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewRebarMarkServices;
internal class GeneralViewRebarPerpMarkService {
    private readonly ViewPointsAnalyzer _viewPointsAnalyzer;
    private readonly AnnotationService _annotationService;
    private readonly FamilySymbol _tagSymbolWithStep;
    private readonly FamilySymbol _independentTagSymbol;
    private readonly string _linkToNodeText = "по А";

    internal GeneralViewRebarPerpMarkService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo,
                                PylonView pylonView) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        ViewOfPylon = pylonView;

        _viewPointsAnalyzer = new ViewPointsAnalyzer(ViewOfPylon);
        _annotationService = new AnnotationService(ViewOfPylon);

        // Находим типоразмер марки несущей арматуры для обозначения позиции, диаметра и комментариев арматуры
        // Без засечки на конце
        _tagSymbolWithStep = mvm.SelectedRebarTagTypeWithStep;
        // Находим типоразмер типовой аннотации для метки ГОСТа сварки
        _independentTagSymbol = Repository.FindSymbol(BuiltInCategory.OST_GenericAnnotation, "Без засечки");
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }
    internal PylonView ViewOfPylon { get; set; }


    internal void TryCreateVerticalBarMarks() {
        try {
            var simpleRebars = SheetInfo.RebarInfo.SimpleVerticalRebars;
            if(simpleRebars.Count == 0) { return; }
            var simpleRebarsInView = ViewModel.RebarFinder.GetRebarsFromView(simpleRebars, ViewOfPylon.ViewElement);
            if(simpleRebarsInView.Count == 0) { return; }

            TryCreateVerticalBarMark(simpleRebarsInView, DirectionType.RightTop);
            TryCreateVerticalBarMark(simpleRebarsInView, DirectionType.LeftTop);
        } catch(Exception) { }
    }

    /// <summary>
    /// Создает марки по сварочному узлу с ссылкой на узел легенды
    /// </summary>
    internal void TryCreateWeldingUnitMarks() {
        var platesArray = ViewModel.RebarFinder.GetSimpleRebars(ViewOfPylon.ViewElement,
                                                                          SheetInfo.ProjectSection, 2001);
        if(platesArray.Count == 0) { return; }
        // Получаем референс-элемент
        var plates = _viewPointsAnalyzer.GetElementByDirection(platesArray, DirectionType.Left, true);

        var viewOptions = new Options {
            View = ViewOfPylon.ViewElement,
            ComputeReferences = true,
            IncludeNonVisibleObjects = false
        };
        var platePoints = plates.get_Geometry(viewOptions)?
            .OfType<GeometryInstance>()
            .SelectMany(ge => ge.GetInstanceGeometry())
            .OfType<Solid>()
            .Where(solid => solid?.Volume > 0)
            .Select(solid => solid.ComputeCentroid())
            .ToList();

        foreach(var point in platePoints) {
            var arc = Arc.Create(
                point,
                UnitUtilsHelper.ConvertToInternalValue(100),
                0,
                2 * Math.PI,
                ViewOfPylon.ViewElement.RightDirection,
                ViewOfPylon.ViewElement.UpDirection);
            Repository.Document.Create.NewDetailCurve(ViewOfPylon.ViewElement, arc);

            var leaderPoint = _viewPointsAnalyzer.GetPointByDirection(point, DirectionType.LeftTop,
                                                                      UnitUtilsHelper.ConvertToInternalValue(70.7),
                                                                      UnitUtilsHelper.ConvertToInternalValue(70.7));
            var bodyPoint = _viewPointsAnalyzer.GetPointByDirection(point, DirectionType.LeftTop, 0.8, 0.6);
            _annotationService.CreateUniversalTag(bodyPoint, _independentTagSymbol, leaderPoint,
                                                  UnitUtilsHelper.ConvertToInternalValue(10), _linkToNodeText);
        }
    }

    /// <summary>
    /// Создает марку по арматурному стержню, расположенному в зависимости от переданного направления на виде
    /// </summary>
    private void TryCreateVerticalBarMark(List<Element> rebars, DirectionType directionType) {
        try {
            // Получаем референс-элемент
            var verticalBar = _viewPointsAnalyzer.GetElementByDirection(rebars, directionType, false);
            // Получаем точку в которую нужно поставить аннотацию
            var point = _viewPointsAnalyzer.GetPointByDirection(verticalBar, directionType, 0, 0, true);
            // Корректируем положение точки, куда будет установлена марка (текст)
            point = _viewPointsAnalyzer.GetPointByDirection(point, directionType, 0.6, 0.3);
            // Создаем марку арматуры
            var tag = _annotationService.CreateRebarTag(point, _tagSymbolWithStep, verticalBar);
            tag.LeaderEndCondition = LeaderEndCondition.Free;
        } catch(Exception) { }
    }
}
