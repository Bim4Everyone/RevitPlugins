using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices;
internal class GeneralViewMarkService {
    private readonly int _formNumberForClampsMax = 1599;
    private readonly int _formNumberForClampsMin = 1500;

    private readonly FamilySymbol _tagSkeletonSymbol;
    private readonly FamilySymbol _tagSymbolWithoutSerif;
    private readonly FamilySymbol _gostTagSymbol;
    private readonly FamilySymbol _breakLineSymbol;

    private readonly ViewPointsAnalyzer _viewPointsAnalyzer;
    private readonly AnnotationService _annotationService;

    // Отступы для формирования линий обрыва
    private readonly double _breakLinesOffsetX = 0.6;
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
        _tagSkeletonSymbol = Repository.FindSymbol(BuiltInCategory.OST_RebarTags, "Изделие_Марка - Полка 30");

        // Находим типоразмер марки несущей арматуры для обозначения позиции, диаметра и комментариев арматуры
        // Без засечки на конце
        _tagSymbolWithoutSerif = Repository.FindSymbol(BuiltInCategory.OST_RebarTags, "Поз., Диаметр / Шаг - Полка 10");
        // Находим типоразмер типовой аннотации для метки ГОСТа сварки
        _gostTagSymbol = Repository.FindSymbol(BuiltInCategory.OST_GenericAnnotation, "Без засечки");

        // Находим типоразмер аннотации линии разрыва
        _breakLineSymbol = Repository.FindSymbol(BuiltInCategory.OST_DetailComponents, "Линейный обрыв");
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
                ReferenceArray refArraySide = dimensionBaseService.GetDimensionRefs(hostElem, '#', '/', 
                                                                                    ["горизонт", "край"]);
                foreach(Reference reference in refArraySide) {
                    SpotDimension spotElevation = Repository.Document.Create.NewSpotElevation(
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
    internal void CreateSkeletonMark() {
        var simpleRebars = SheetInfo.RebarInfo.SimpleVerticalRebars;
        if(simpleRebars.Count == 0) { return; }

        // Получаем референс-элемент
        Element rightVerticalBar = _viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.Right,
                                                                            false);
        // Получаем точку в которую нужно поставить аннотацию
        var pointRight= _viewPointsAnalyzer.GetPointByDirection(rightVerticalBar, DirectionType.Right,
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
    }

    /// <summary>
    /// Создает марки хомутов на основном виде опалубки в зависимости от положения на виде
    /// </summary>
    internal void CreateClampMarks() {
        var simpleClamps = ViewModel.RebarFinder.GetSimpleRebars(ViewOfPylon.ViewElement, SheetInfo.ProjectSection,
                                                                 _formNumberForClampsMin, _formNumberForClampsMax);

        var pointForCompare = _viewPointsAnalyzer.GetTransformedPoint(SheetInfo.RebarInfo.SkeletonParentRebar, true);
        foreach(var simpleClamp in simpleClamps) {
            var clampPoint = _viewPointsAnalyzer.GetTransformedPoint(simpleClamp, true);
            if(clampPoint.X < pointForCompare.X) {
                CreateClampMark(simpleClamp, DirectionType.LeftTop);
            } else {
                CreateClampMark(simpleClamp, DirectionType.RightTop);
            }
        }
    }

    /// <summary>
    /// Создает марку хомута на основном виде опалубки
    /// </summary>
    private void CreateClampMark(Element simpleClamp, DirectionType directionType) {
        // Получаем точку в которую нужно поставить аннотацию
        var annotPoint = _viewPointsAnalyzer.GetPointByDirection(simpleClamp, directionType, 0, 0, true);
        // Корректируем положение точки, куда будет установлена марка (текст)
        annotPoint = _viewPointsAnalyzer.GetPointByDirection(annotPoint, directionType, 2.4, 0.3);

        // Создаем марку арматуры
        var clampTag = _annotationService.CreateRebarTag(annotPoint, _tagSymbolWithoutSerif, simpleClamp);
        clampTag.LeaderEndCondition = LeaderEndCondition.Free;
    }

    internal void CreateAdditionalMark() {
        // Получаем референс-элемент
        var bottomElement = SheetInfo.HostElems.First();

        // Получаем точку в которую нужно поставить аннотацию
        var annotPoint = _viewPointsAnalyzer.GetPointByDirection(bottomElement, DirectionType.RightBottom,
                                                                 5, 1, false);
        var leaderPoint = _viewPointsAnalyzer.GetPointByDirection(bottomElement, DirectionType.RightBottom,
                                                                 2.5, 0.5, false);
        // Создаем типовую аннотацию для обозначения ГОСТа
        _annotationService.CreateUniversalTag(annotPoint, _gostTagSymbol, leaderPoint, 
                                              UnitUtilsHelper.ConvertToInternalValue(40),
                                              "Арматурные выпуски", "показаны условно");

        CreateLowerBreakLines();
        CreateUpperBreakLines();
        CreateMiddleBreakLines();
    }

    /// <summary>
    /// Создаем линии обрыва ниже первого опалубочного элемента
    /// </summary>
    private void CreateLowerBreakLines() {
        var view = ViewOfPylon.ViewElement;
        // Т.к. линии обрыва будут снизу, то берем первый элемент пилона
        var bottomElement = SheetInfo.HostElems.First();

        // Получаем спроецированные на плоскость вида граничные точки
        var bbMin = bottomElement.get_BoundingBox(view).Min;
        bbMin = _viewPointsAnalyzer.ProjectPointToViewFront(view, bbMin);
        var bbMax = bottomElement.get_BoundingBox(view).Max;
        bbMax = _viewPointsAnalyzer.ProjectPointToViewFront(view, bbMax);
        var bottomRightPt = new XYZ(bbMax.X, bbMax.Y, bbMin.Z);

        // Получаем точки со смещением относительно граничных точек опалубки для размещения аннотаций
        var point1 = _viewPointsAnalyzer.GetPointByDirection(bbMin, DirectionType.Left, 
                                                             _breakLinesOffsetX, _breakLinesOffsetBottomY);
        var point2 = _viewPointsAnalyzer.GetPointByDirection(bbMin, DirectionType.LeftBottom, 
                                                             _breakLinesOffsetX, _breakLinesOffsetBottomY);
        var point3 = _viewPointsAnalyzer.GetPointByDirection(bottomRightPt, DirectionType.RightBottom, 
                                                             _breakLinesOffsetX, _breakLinesOffsetBottomY);
        var point4 = _viewPointsAnalyzer.GetPointByDirection(bottomRightPt, DirectionType.Right, 
                                                             _breakLinesOffsetX, _breakLinesOffsetBottomY);
        // Линии обрыва - это 2D-аннотационные семейства на основе линии
        var line1 = Line.CreateBound(point1, point2);
        var line2 = Line.CreateBound(point2, point3);
        var line3 = Line.CreateBound(point3, point4);
        Repository.Document.Create.NewFamilyInstance(line1, _breakLineSymbol, view);
        Repository.Document.Create.NewFamilyInstance(line2, _breakLineSymbol, view);
        Repository.Document.Create.NewFamilyInstance(line3, _breakLineSymbol, view);
    }

    /// <summary>
    /// Создаем линии обрыва выше последнего опалубочного элемента
    /// </summary>
    private void CreateUpperBreakLines() {
        var view = ViewOfPylon.ViewElement;
        // Т.к. линии обрыва будут сверху, то берем последний элемент пилона
        var topElement = SheetInfo.HostElems.Last();

        // Получаем спроецированные на плоскость вида граничные точки
        var bbMin = topElement.get_BoundingBox(view).Min;
        bbMin = _viewPointsAnalyzer.ProjectPointToViewFront(view, bbMin);
        var bbMax = topElement.get_BoundingBox(view).Max;
        bbMax = _viewPointsAnalyzer.ProjectPointToViewFront(view, bbMax);
        var topLeftPt = new XYZ(bbMin.X, bbMin.Y, bbMax.Z);

        // Получаем точки со смещением относительно граничных точек опалубки для размещения аннотаций
        var point1 = _viewPointsAnalyzer.GetPointByDirection(bbMax, DirectionType.Right, 
                                                             _breakLinesOffsetX, _breakLinesOffsetTopY);
        var point2 = _viewPointsAnalyzer.GetPointByDirection(bbMax, DirectionType.RightTop, 
                                                             _breakLinesOffsetX, _breakLinesOffsetTopY);
        var point3 = _viewPointsAnalyzer.GetPointByDirection(topLeftPt, DirectionType.LeftTop, 
                                                             _breakLinesOffsetX, _breakLinesOffsetTopY);
        var point4 = _viewPointsAnalyzer.GetPointByDirection(topLeftPt, DirectionType.Left, 
                                                             _breakLinesOffsetX, _breakLinesOffsetTopY);
        // Линии обрыва - это 2D-аннотационные семейства на основе линии
        var line1 = Line.CreateBound(point1, point2);
        var line2 = Line.CreateBound(point2, point3);
        var line3 = Line.CreateBound(point3, point4);
        Repository.Document.Create.NewFamilyInstance(line1, _breakLineSymbol, view);
        Repository.Document.Create.NewFamilyInstance(line2, _breakLineSymbol, view);
        Repository.Document.Create.NewFamilyInstance(line3, _breakLineSymbol, view);
    }

    /// <summary>
    /// Создаем линии обрыва между опалубочными элементами, если их несколько
    /// </summary>
    private void CreateMiddleBreakLines() {
        if(SheetInfo.HostElems.Count == 1) {
            return;
        }
        var view = ViewOfPylon.ViewElement;
        var previousElement = SheetInfo.HostElems.First();

        for(int i = 1; i < SheetInfo.HostElems.Count; i++) {
            var currentElement = SheetInfo.HostElems[i];

            // Получаем спроецированные на плоскость вида граничные точки
            var currentBbMax = currentElement.get_BoundingBox(view).Max;
            var currentBbMin = currentElement.get_BoundingBox(view).Min;
            var previousBbMax = previousElement.get_BoundingBox(view).Max;
            var previousBbMin = previousElement.get_BoundingBox(view).Min;

            currentBbMax = _viewPointsAnalyzer.ProjectPointToViewFront(view, currentBbMax);
            currentBbMin = _viewPointsAnalyzer.ProjectPointToViewFront(view, currentBbMin);
            previousBbMax = _viewPointsAnalyzer.ProjectPointToViewFront(view, previousBbMax);
            previousBbMin = _viewPointsAnalyzer.ProjectPointToViewFront(view, previousBbMin);

            var currentBottomRightPt = new XYZ(currentBbMax.X, currentBbMax.Y, currentBbMin.Z);
            var previousTopLeftPt = new XYZ(previousBbMin.X, previousBbMin.Y, previousBbMax.Z);

            // Получаем точки со смещением относительно граничных точек опалубки для размещения аннотаций
            var point1 = _viewPointsAnalyzer.GetPointByDirection(previousBbMax, DirectionType.Right, 
                                                                 _breakLinesOffsetX, _breakLinesOffsetBottomY);
            var point2 = _viewPointsAnalyzer.GetPointByDirection(currentBottomRightPt, DirectionType.Right, 
                                                                 _breakLinesOffsetX, _breakLinesOffsetTopY);
            var point3 = _viewPointsAnalyzer.GetPointByDirection(currentBbMin, DirectionType.Left, 
                                                                 _breakLinesOffsetX, _breakLinesOffsetTopY);
            var point4 = _viewPointsAnalyzer.GetPointByDirection(previousTopLeftPt, DirectionType.Left, 
                                                                 _breakLinesOffsetX, _breakLinesOffsetBottomY);
            // Линии обрыва - это 2D-аннотационные семейства на основе линии
            var line1 = Line.CreateBound(point1, point2);
            var line2 = Line.CreateBound(point3, point4);
            Repository.Document.Create.NewFamilyInstance(line1, _breakLineSymbol, view);
            Repository.Document.Create.NewFamilyInstance(line2, _breakLineSymbol, view);

            previousElement = currentElement;
        }
    }
}
