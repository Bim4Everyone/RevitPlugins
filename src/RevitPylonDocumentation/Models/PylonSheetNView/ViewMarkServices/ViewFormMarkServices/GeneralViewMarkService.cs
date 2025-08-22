using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewFormMarkServices;
internal class GeneralViewMarkService {
    private readonly int _formNumberForClampsMax = 1599;
    private readonly int _formNumberForClampsMin = 1500;

    private readonly FamilySymbol _tagSkeletonSymbol;
    private readonly FamilySymbol _tagSymbolWithStep;
    private readonly FamilySymbol _gostTagSymbol;
    private readonly FamilySymbol _breakLineSymbol;
    private readonly FamilySymbol _concretingSeamSymbol;

    private readonly ViewPointsAnalyzer _viewPointsAnalyzer;
    private readonly AnnotationService _annotationService;

    // Отступы для формирования линий обрыва
    private readonly double _breakLinesOffsetX = 0.55;
    private readonly double _breakLinesOffsetTopY = 4;
    private readonly double _breakLinesOffsetBottomY = 1.2;

    internal GeneralViewMarkService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                    PylonView pylonView) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        ViewOfPylon = pylonView;

        _viewPointsAnalyzer = new ViewPointsAnalyzer(ViewOfPylon);
        _annotationService = new AnnotationService(ViewOfPylon);

        // Находим типоразмер марки несущей арматуры для обозначения марки изделия
        _tagSkeletonSymbol = mvm.SelectedSkeletonTagType;

        // Находим типоразмер марки несущей арматуры для обозначения позиции, диаметра и комментариев арматуры
        // Без засечки на конце
        _tagSymbolWithStep = mvm.SelectedRebarTagTypeWithStep;
        // Находим типоразмер типовой аннотации для метки ГОСТа сварки
        _gostTagSymbol = Repository.FindSymbol(BuiltInCategory.OST_GenericAnnotation, "Без засечки");

        // Находим типоразмер аннотации линии разрыва
        _breakLineSymbol = mvm.SelectedBreakLineType;

        // Находим типоразмер аннотации рабочего шва бетонирования
        _concretingSeamSymbol = mvm.SelectedConcretingJointType;
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }
    internal PylonView ViewOfPylon { get; set; }


    /// <summary>
    /// Метод по созданию размеров по опалубке пилонов
    /// </summary>
    /// <param name="view">Вид, на котором нужно создать размеры</param>
    /// <param name="clampsParentRebars">Список экземпляров семейств пилонов</param>
    /// <param name="dimensionBaseService">Сервис по анализу основ размеров</param>
    internal void TryCreatePylonElevMark(List<Element> hostElems, DimensionBaseService dimensionBaseService) {
        try {
            var location = dimensionBaseService.GetDimensionLine(hostElems.First() as FamilyInstance, 
                                                                 DimensionOffsetType.Left, 2).Origin;
            foreach(var item in hostElems) {
                if(item is not FamilyInstance hostElem) { return; }

                // Собираем опорные плоскости по опалубке, например:
                // #_1_горизонт_край_низ
                // #_1_горизонт_край_верх
                var refArraySide = dimensionBaseService.GetDimensionRefs(hostElem, '#', '/', 
                                                                                    ["горизонт", "край"]);
                foreach(Reference reference in refArraySide) {
                    var spotElevation = Repository.Document.Create.NewSpotElevation(
                        ViewOfPylon.ViewElement,
                        reference,
                        location,
                        location,
                        location,
                        location,
                        false);
                    spotElevation.ChangeTypeId(ViewModel.SelectedSpotDimensionType.Id);
                }
            }
        } catch(Exception) { }
    }

    /// <summary>
    /// Создает марку арматурного каркаса на основном виде опалубки
    /// </summary>
    internal void TryCreateSkeletonMark() {
        var simpleRebars = SheetInfo.RebarInfo.SimpleVerticalRebars;
        if(simpleRebars.Count == 0) { return; }
        try {
            // Получаем референс-элемент
            var rightVerticalBar = _viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.Right,
                                                                                 false);
            // Получаем точку в которую нужно поставить аннотацию
            var pointRight = _viewPointsAnalyzer.GetPointByDirection(rightVerticalBar, DirectionType.Right,
                                                                     0, 0, true);
            // Корректируем положение точки, куда будет установлена марка (текст)
            pointRight = _viewPointsAnalyzer.GetPointByDirection(pointRight, DirectionType.RightBottom, 1.5, 3.5);
            // Создаем марку арматуры
            var rightTag = _annotationService.CreateRebarTag(pointRight, _tagSkeletonSymbol, rightVerticalBar);

#if REVIT_2022_OR_GREATER
            rightTag.LeaderEndCondition = LeaderEndCondition.Free;
            var rightVerticalBarRef = new Reference(rightVerticalBar);

            var tagLeaderEnd = rightTag.GetLeaderEnd(rightVerticalBarRef);
            tagLeaderEnd = _viewPointsAnalyzer.GetPointByDirection(tagLeaderEnd, DirectionType.Bottom, 0, 3);
            rightTag.SetLeaderEnd(rightVerticalBarRef, tagLeaderEnd);
#endif
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

    internal void TryCreateAdditionalMark() {
        try {
            var view = ViewOfPylon.ViewElement;
            
            // Получаем референс-элемент
            var bottomElement = SheetInfo.HostElems.First();

            // Получаем спроецированные на плоскость вида граничные точки
            var bbMin = bottomElement.get_BoundingBox(view).Min;
            bbMin = _viewPointsAnalyzer.ProjectPointToViewFront(bbMin);
            var bbMax = bottomElement.get_BoundingBox(view).Max;
            bbMax = _viewPointsAnalyzer.ProjectPointToViewFront(bbMax);
            var bottomRightPt = new XYZ(bbMax.X, bbMax.Y, bbMin.Z);

            // Получаем точки со смещением относительно граничных точек опалубки для размещения аннотаций
            var annotPoint = _viewPointsAnalyzer.GetPointByDirection(bottomRightPt, DirectionType.RightBottom,
                                                                     2, 1.1);
            var leaderPoint = _viewPointsAnalyzer.GetPointByDirection(bottomRightPt, DirectionType.LeftBottom,
                                                                      0.4, 0.4);
            // Создаем типовую аннотацию для обозначения ГОСТа
            _annotationService.CreateUniversalTag(annotPoint, _gostTagSymbol, leaderPoint,
                                                  UnitUtilsHelper.ConvertToInternalValue(40),
                                                  "Арматурные выпуски", "показаны условно");
        } catch(Exception) { }
    }

    /// <summary>
    /// Создаем линии обрыва ниже первого опалубочного элемента
    /// </summary>
    internal void TryCreateLowerBreakLines() {
        if(_breakLineSymbol is null) { return; }
        var view = ViewOfPylon.ViewElement;
        try {
            // Т.к. линии обрыва будут снизу, то берем первый элемент пилона
            var bottomElement = SheetInfo.HostElems.First();

            // Получаем спроецированные на плоскость вида граничные точки
            var bbMin = bottomElement.get_BoundingBox(view).Min;
            bbMin = _viewPointsAnalyzer.ProjectPointToViewFront(bbMin);
            var bbMax = bottomElement.get_BoundingBox(view).Max;
            bbMax = _viewPointsAnalyzer.ProjectPointToViewFront(bbMax);
            var bottomRightPt = new XYZ(bbMax.X, bbMax.Y, bbMin.Z);

            // Получаем точки со смещением относительно граничных точек опалубки для размещения аннотаций
            var point1 = _viewPointsAnalyzer.GetPointByDirection(bbMin, DirectionType.LeftTop,
                                                                 _breakLinesOffsetX, 0.1);
            var point2 = _viewPointsAnalyzer.GetPointByDirection(bbMin, DirectionType.LeftBottom,
                                                                 _breakLinesOffsetX, _breakLinesOffsetBottomY);
            var point3 = _viewPointsAnalyzer.GetPointByDirection(bottomRightPt, DirectionType.RightBottom,
                                                                 _breakLinesOffsetX, _breakLinesOffsetBottomY);
            var point4 = _viewPointsAnalyzer.GetPointByDirection(bottomRightPt, DirectionType.RightTop,
                                                                 _breakLinesOffsetX, 0.1);
            // Линии обрыва - это 2D-аннотационные семейства на основе линии
            var line1 = Line.CreateBound(point1, point2);
            var line2 = Line.CreateBound(point2, point3);
            var line3 = Line.CreateBound(point3, point4);
            Repository.Document.Create.NewFamilyInstance(line1, _breakLineSymbol, view);
            Repository.Document.Create.NewFamilyInstance(line2, _breakLineSymbol, view);
            Repository.Document.Create.NewFamilyInstance(line3, _breakLineSymbol, view);
        } catch(Exception) { }
    }

    /// <summary>
    /// Создаем линии обрыва выше последнего опалубочного элемента
    /// </summary>
    internal void TryCreateUpperBreakLines() {
        if(_breakLineSymbol is null) { return; }
        var view = ViewOfPylon.ViewElement;
        try {
            // Т.к. линии обрыва будут сверху, то берем последний элемент пилона
            var topElement = SheetInfo.HostElems.Last();

            // Получаем спроецированные на плоскость вида граничные точки
            var bbMin = topElement.get_BoundingBox(view).Min;
            bbMin = _viewPointsAnalyzer.ProjectPointToViewFront(bbMin);
            var bbMax = topElement.get_BoundingBox(view).Max;
            bbMax = _viewPointsAnalyzer.ProjectPointToViewFront(bbMax);
            var topLeftPt = new XYZ(bbMin.X, bbMin.Y, bbMax.Z);

            // Получаем точки со смещением относительно граничных точек опалубки для размещения аннотаций
            var point1 = _viewPointsAnalyzer.GetPointByDirection(bbMax, DirectionType.RightBottom,
                                                                 _breakLinesOffsetX, 0.1);
            var point2 = _viewPointsAnalyzer.GetPointByDirection(bbMax, DirectionType.RightTop,
                                                                 _breakLinesOffsetX, _breakLinesOffsetTopY);
            var point3 = _viewPointsAnalyzer.GetPointByDirection(topLeftPt, DirectionType.LeftTop,
                                                                 _breakLinesOffsetX, _breakLinesOffsetTopY);
            var point4 = _viewPointsAnalyzer.GetPointByDirection(topLeftPt, DirectionType.LeftBottom,
                                                                 _breakLinesOffsetX, 0.1);
            // Линии обрыва - это 2D-аннотационные семейства на основе линии
            var line1 = Line.CreateBound(point1, point2);
            var line2 = Line.CreateBound(point2, point3);
            var line3 = Line.CreateBound(point3, point4);
            Repository.Document.Create.NewFamilyInstance(line1, _breakLineSymbol, view);
            Repository.Document.Create.NewFamilyInstance(line2, _breakLineSymbol, view);
            Repository.Document.Create.NewFamilyInstance(line3, _breakLineSymbol, view);
        } catch(Exception) { }
    }

    /// <summary>
    /// Создаем линии обрыва между опалубочными элементами, если их несколько
    /// </summary>
    internal void TryCreateMiddleBreakLines() {
        if(SheetInfo.HostElems.Count == 1 || _breakLineSymbol is null) {
            return;
        }
        var view = ViewOfPylon.ViewElement;
        var previousElement = SheetInfo.HostElems.First();
        try {
            for(int i = 1; i < SheetInfo.HostElems.Count; i++) {
                var currentElement = SheetInfo.HostElems[i];

                // Получаем спроецированные на плоскость вида граничные точки
                var currentBbMax = currentElement.get_BoundingBox(view).Max;
                var currentBbMin = currentElement.get_BoundingBox(view).Min;
                var previousBbMax = previousElement.get_BoundingBox(view).Max;
                var previousBbMin = previousElement.get_BoundingBox(view).Min;

                currentBbMax = _viewPointsAnalyzer.ProjectPointToViewFront(currentBbMax);
                currentBbMin = _viewPointsAnalyzer.ProjectPointToViewFront(currentBbMin);
                previousBbMax = _viewPointsAnalyzer.ProjectPointToViewFront(previousBbMax);
                previousBbMin = _viewPointsAnalyzer.ProjectPointToViewFront(previousBbMin);

                var currentBottomRightPt = new XYZ(currentBbMax.X, currentBbMax.Y, currentBbMin.Z);
                var previousTopLeftPt = new XYZ(previousBbMin.X, previousBbMin.Y, previousBbMax.Z);

                // Получаем точки со смещением относительно граничных точек опалубки для размещения аннотаций
                var point1 = _viewPointsAnalyzer.GetPointByDirection(previousBbMax, DirectionType.RightBottom,
                                                                     _breakLinesOffsetX, 0.1);
                var point2 = _viewPointsAnalyzer.GetPointByDirection(currentBottomRightPt, DirectionType.RightTop,
                                                                     _breakLinesOffsetX, 0.1);
                var point3 = _viewPointsAnalyzer.GetPointByDirection(currentBbMin, DirectionType.LeftTop,
                                                                     _breakLinesOffsetX, 0.1);
                var point4 = _viewPointsAnalyzer.GetPointByDirection(previousTopLeftPt, DirectionType.LeftBottom,
                                                                     _breakLinesOffsetX, 0.1);
                // Линии обрыва - это 2D-аннотационные семейства на основе линии
                var line1 = Line.CreateBound(point1, point2);
                var line2 = Line.CreateBound(point3, point4);
                Repository.Document.Create.NewFamilyInstance(line1, _breakLineSymbol, view);
                Repository.Document.Create.NewFamilyInstance(line2, _breakLineSymbol, view);

                previousElement = currentElement;
            }
        } catch(Exception) { }
    }

    internal void TryCreateConcretingSeams() {
        var view = ViewOfPylon.ViewElement;
        if(_concretingSeamSymbol is null) { return; }

        try {
            foreach(var pylon in SheetInfo.HostElems) {
                // Получаем спроецированные на плоскость вида граничные точки
                var bbMax = pylon.get_BoundingBox(view).Max;
                bbMax = _viewPointsAnalyzer.ProjectPointToViewFront(bbMax);
                var bbMin = pylon.get_BoundingBox(view).Min;
                bbMin = _viewPointsAnalyzer.ProjectPointToViewFront(bbMin);
                var topLeftPt = new XYZ(bbMin.X, bbMin.Y, bbMax.Z);
                var bottomRightPt = new XYZ(bbMax.X, bbMax.Y, bbMin.Z);

                // Аннотации швов бетонирования - это 2D-аннотационные семейства на основе линии
                var line1 = Line.CreateBound(bbMin, bottomRightPt);
                var line2 = Line.CreateBound(bbMax, topLeftPt);
                Repository.Document.Create.NewFamilyInstance(line1, _concretingSeamSymbol, view);
                Repository.Document.Create.NewFamilyInstance(line2, _concretingSeamSymbol, view);
            }
        } catch(Exception) { }
    }
}
