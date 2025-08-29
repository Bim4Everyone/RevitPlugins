using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewRebarMarkServices;
internal class GeneralViewRebarMarkService {
    private readonly ViewPointsAnalyzer _viewPointsAnalyzer;
    private readonly AnnotationService _annotationService;
    private readonly FamilySymbol _tagSymbolWithStep;

    private readonly int _formNumberForClampsMax = 1599;
    private readonly int _formNumberForClampsMin = 1500;

    internal GeneralViewRebarMarkService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo,
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

    /// <summary>
    /// Создает марки хомутов на основном виде опалубки в зависимости от положения на виде
    /// </summary>
    internal void TryCreateClampMarks(bool isFrontView) {
        try {
            var simpleClamps = ViewModel.RebarFinder.GetSimpleRebars(ViewOfPylon.ViewElement, SheetInfo.ProjectSection,
                                                                     _formNumberForClampsMin, _formNumberForClampsMax);

            var pointForCompare = _viewPointsAnalyzer.GetTransformedPoint(SheetInfo.RebarInfo.SkeletonParentRebar, true);
            foreach(var simpleClamp in simpleClamps) {
                var clampPoint = _viewPointsAnalyzer.GetTransformedPoint(simpleClamp, true);
                if(!isFrontView || clampPoint.X > pointForCompare.X) {
                    TryCreateClampMark(simpleClamp, DirectionType.RightTop, isFrontView);
                } else {
                    TryCreateClampMark(simpleClamp, DirectionType.LeftTop, isFrontView);
                }
            }
        } catch(Exception) { }
    }

    /// <summary>
    /// Создает марку хомута на основном виде опалубки
    /// </summary>
    private void TryCreateClampMark(Element simpleClamp, DirectionType directionType, bool isFrontView) {
        try {
            var xOffset = isFrontView ? 2.4 : 1;
            // Получаем точку в которую нужно поставить аннотацию
            var annotPoint = _viewPointsAnalyzer.GetPointByDirection(simpleClamp, directionType, 0, 0, true);
            // Корректируем положение точки, куда будет установлена марка (текст)
            annotPoint = _viewPointsAnalyzer.GetPointByDirection(annotPoint, directionType, xOffset, 0.3);

            // Создаем марку арматуры
            var clampTag = _annotationService.CreateRebarTag(annotPoint, _tagSymbolWithStep, simpleClamp);
            clampTag.LeaderEndCondition = LeaderEndCondition.Free;
        } catch(Exception) { }
    }
}
