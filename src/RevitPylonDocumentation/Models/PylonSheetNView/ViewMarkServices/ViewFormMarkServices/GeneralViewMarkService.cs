using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewFormMarkServices;
internal class GeneralViewMarkService {
    private readonly FamilySymbol _tagSkeletonSymbol;
    private readonly FamilySymbol _universalTagType;
    private readonly FamilySymbol _breakLineSymbol;
    private readonly FamilySymbol _concretingSeamSymbol;

    private readonly ViewPointsAnalyzer _viewPointsAnalyzer;
    private readonly TagCreationService _annotationService;
    private readonly DimensionBaseService _dimensionBaseService;

    // Отступы для формирования линий обрыва
    private readonly double _breakLinesOffsetX = 0.5;
    private readonly double _breakLinesOffsetY = 0.3;
    private readonly double _breakLinesOffsetYBottom = 1;

    // Отступы области скрытия линии обрыва
    private readonly string _breakLineDepthParamName = "Глубина маскировки";
    private readonly double _breakLineDepthParamValue = 1500;
    private readonly string _breakLineLengthLeftParamName = "Длина маскировки_влево";
    private readonly double _breakLineLengthLeftParamValue = 100;
    private readonly string _breakLineLengthRightParamName = "Длина маскировки_право";
    private readonly double _breakLineLengthRightParamValue = 100;

    internal GeneralViewMarkService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                    PylonView pylonView, DimensionBaseService dimensionBaseService) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        ViewOfPylon = pylonView;
        _dimensionBaseService = dimensionBaseService;

        _viewPointsAnalyzer = new ViewPointsAnalyzer(ViewOfPylon);
        _annotationService = new TagCreationService(ViewOfPylon);

        // Находим типоразмер марки несущей арматуры для обозначения марки изделия
        _tagSkeletonSymbol = mvm.SelectedSkeletonTagType;

        // Находим типоразмер марки несущей арматуры для обозначения позиции, диаметра и комментариев арматуры
        // Находим типоразмер типовой аннотации для метки ГОСТа сварки
        _universalTagType = mvm.SelectedUniversalTagType;

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
    internal void TryCreatePylonElevMark(List<Element> hostElems) {
        try {
            var location = _dimensionBaseService.GetDimensionLine(hostElems.First() as FamilyInstance, 
                                                                 DirectionType.Left, 2).Origin;
            foreach(var item in hostElems) {
                if(item is not FamilyInstance hostElem) { return; }

                // Собираем опорные плоскости по опалубке, например:
                // #_1_горизонт_край_низ
                // #_1_горизонт_край_верх
                var refArraySide = _dimensionBaseService.GetDimensionRefs(hostElem, ["горизонт", "край"]);
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
    internal void TryCreateSkeletonMark(bool isForPerpView) {
        var simpleRebars = SheetInfo.RebarInfo.SimpleVerticalRebars;
        if(simpleRebars.Count == 0) { return; }
        var simpleRebarsInView = ViewModel.RebarFinder.GetRebarsFromView(simpleRebars, ViewOfPylon.ViewElement);
        if(simpleRebarsInView.Count == 0) { return; }

        try {
            // Получаем референс-элемент
            var rightVerticalBar = _viewPointsAnalyzer.GetElementByDirection(simpleRebarsInView, DirectionType.Right,
                                                                             false);
            // Получаем точку в которую нужно поставить аннотацию
            var point = _viewPointsAnalyzer.GetPointByDirection(rightVerticalBar, DirectionType.Right, 0, 0, true);
            // Корректируем положение точки, куда будет установлена марка (текст)
            point = _viewPointsAnalyzer.GetPointByDirection(point, DirectionType.RightBottom, 0.8, 3.2);
            // Корректируем положение точки, если она слишком удалена от пилона (из-за семейства Гэшки)
            if(isForPerpView) {
#if REVIT_2022_OR_GREATER
                var hostOrigin = SheetInfo.ElemsInfo.HostOrigin;
                var hostOriginProjected = _viewPointsAnalyzer.ProjectPointToViewFront(hostOrigin);

                var pointProjected = _viewPointsAnalyzer.ProjectPointToViewFront(point);
                pointProjected = _viewPointsAnalyzer.ProjectPointToHorizontalPlane(hostOriginProjected, pointProjected);
                var dist = pointProjected.DistanceTo(hostOriginProjected);

                var hostWidth = SheetInfo.ElemsInfo.HostWidth;
                if(dist > hostWidth) {
                    point = _viewPointsAnalyzer.GetPointByDirection(
                        new XYZ(hostOriginProjected.X, hostOriginProjected.Y, point.Z), DirectionType.Right, hostWidth, 0);
                }
#endif
            }
            // Создаем марку арматуры
            var rightTag = _annotationService.CreateRebarTag(point, _tagSkeletonSymbol, rightVerticalBar);
            rightTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
            rightTag.LeaderEndCondition = LeaderEndCondition.Free;
            var rightVerticalBarRef = new Reference(rightVerticalBar);

            var tagLeaderEnd = rightTag.GetLeaderEnd(rightVerticalBarRef);
            tagLeaderEnd = _viewPointsAnalyzer.GetPointByDirection(tagLeaderEnd, DirectionType.Bottom, 0, 3);
            rightTag.SetLeaderEnd(rightVerticalBarRef, tagLeaderEnd);
#endif
        } catch(Exception) { }
    }

    internal void TryCreateAdditionalMark(bool isForPerpView) {
        try {
            // Если в пилоне есть семейство при помощи которого армируются пилоны паркинга, то выпусков снизу не будет
            // А это значит, что и данная марка не нужна
            if(SheetInfo.RebarInfo.SkeletonParentRebarForParking) { return; }
            
            var view = ViewOfPylon.ViewElement;
            // Определяем отступ от пилона по горизонтали
            double horizOriginOffset = isForPerpView 
                                            ? SheetInfo.ElemsInfo.HostWidth * 0.5
                                            : SheetInfo.ElemsInfo.HostLength * 0.5;
            var origin = SheetInfo.ElemsInfo.HostOrigin;
            // Получаем спроецированную на плоскость вида граничную точку пилона
            var pylonRightMinPoint = _viewPointsAnalyzer.GetPointByDirection(origin, DirectionType.Right,
                                                                             horizOriginOffset, 0);
            pylonRightMinPoint = _viewPointsAnalyzer.ProjectPointToViewFront(pylonRightMinPoint);
            // Определяем точки аннотации
            var leaderPoint = _viewPointsAnalyzer.GetPointByDirection(pylonRightMinPoint, DirectionType.LeftBottom,
                                                                      0.2, 0.5);
            var annotPoint = _viewPointsAnalyzer.GetPointByDirection(pylonRightMinPoint, DirectionType.RightBottom,
                                                                     1.8, 1.1);
            // Создаем типовую аннотацию для обозначения ГОСТа
            _annotationService.CreateUniversalTag(annotPoint, _universalTagType, leaderPoint,
                                                  UnitUtilsHelper.ConvertToInternalValue(40),
                                                  "Арматурные выпуски", "нижнего пилона");
        } catch(Exception) { }
    }

    /// <summary>
    /// Создаем линии обрыва ниже первого опалубочного элемента
    /// </summary>
    internal void TryCreateLowerBreakLines(bool isForPerpView) {
        if(_breakLineSymbol is null) { return; }
        var view = ViewOfPylon.ViewElement;
        try {
            var origin = SheetInfo.ElemsInfo.HostOrigin;

            // Получаем координаты точек max и min рамки подрезки вида в координатах проекта
            // Проецируем их на плоскость вида, чтобы корректно размещать аннотации
            var viewMax = _viewPointsAnalyzer.ProjectPointToViewFront(
                view.CropBox.Transform.OfPoint(view.CropBox.Max));
            var viewMin = _viewPointsAnalyzer.ProjectPointToViewFront(
                view.CropBox.Transform.OfPoint(view.CropBox.Min));

            XYZ leftPoint;
            XYZ rightPoint;
            if(isForPerpView && SheetInfo.RebarInfo.SkeletonParentRebarForParking) {
                // Если условие выполняется, то нужно боковые точки брать не относительно габарита пилона
                // а от рамки подрезки вида, т.к. она учитывает выпуски вертикального армирования в плиту
                // Т.к. точки CropBox, уже спроецированы на плоскость вида, то задаем отступ
                rightPoint = _viewPointsAnalyzer.GetPointByDirection(viewMax, DirectionType.Left, 
                                                                     _breakLinesOffsetX, 0);
                leftPoint = _viewPointsAnalyzer.GetPointByDirection(viewMin, DirectionType.Right, 
                                                                    _breakLinesOffsetX, 0);
                // Проецируем на плоскость низа пилона (по Origin)
                rightPoint = _viewPointsAnalyzer.ProjectPointToHorizontalPlane(origin, rightPoint);
                leftPoint = _viewPointsAnalyzer.ProjectPointToHorizontalPlane(origin, leftPoint);
            } else {
                // В этом случае боковые точки будут получены относительно габаритов пилона
                // Определяем отступ от пилона по горизонтали в зависимости от типа вида
                double horizOriginOffset = isForPerpView
                    ? SheetInfo.ElemsInfo.HostWidth * 0.5 + _breakLinesOffsetY
                    : SheetInfo.ElemsInfo.HostLength * 0.5 + _breakLinesOffsetY;

                // Получаем спроецированную на плоскость вида правую граничную точку пилона с учетом отступа
                var pylonRightMinPoint = _viewPointsAnalyzer.GetPointByDirection(origin, DirectionType.Right,
                                                                                 horizOriginOffset, 0);
                rightPoint = _viewPointsAnalyzer.ProjectPointToViewFront(pylonRightMinPoint);
                // Получаем спроецированную на плоскость вида левую граничную точку пилона с учетом отступа
                var pylonLeftMinPoint = _viewPointsAnalyzer.GetPointByDirection(origin, DirectionType.Left,
                                                                                horizOriginOffset, 0);
                leftPoint = _viewPointsAnalyzer.ProjectPointToViewFront(pylonLeftMinPoint);
            }

            // Получаем точки со смещением относительно граничных точек опалубки для размещения аннотаций
            // Чтобы их не срезало рамкой подрезки
            // Левая верхняя точка
            var point1 = leftPoint;
            // Левая нижняя точка
            var point2 = _viewPointsAnalyzer.GetPointByDirection(new XYZ(leftPoint.X,
                                                                         leftPoint.Y, 
                                                                         viewMin.Z),
                                                                 DirectionType.Top, 0, _breakLinesOffsetYBottom);
            // Правая нижняя точка
            var point3 = _viewPointsAnalyzer.GetPointByDirection(new XYZ(rightPoint.X,
                                                                         rightPoint.Y, 
                                                                         viewMin.Z),
                                                                 DirectionType.Top, 0, _breakLinesOffsetYBottom);
            // Правая верхняя точка
            var point4 = rightPoint;
            
            // Линии обрыва - это 2D-аннотационные семейства на основе линии
            var line1 = Line.CreateBound(point1, point2);
            var line2 = Line.CreateBound(point2, point3);
            var line3 = Line.CreateBound(point3, point4);
            var breakLine1 = Repository.Document.Create.NewFamilyInstance(line1, _breakLineSymbol, view);
            var breakLine2 = Repository.Document.Create.NewFamilyInstance(line2, _breakLineSymbol, view);
            var breakLine3 = Repository.Document.Create.NewFamilyInstance(line3, _breakLineSymbol, view);

            TrySetBreakLineOffsets(breakLine1);
            TrySetBreakLineOffsets(breakLine2);
            TrySetBreakLineOffsets(breakLine3);
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
            // Отметка Z точки максимума верхнего пилона
            var pylonMaxZ = _viewPointsAnalyzer.ProjectPointToViewFront(
                topElement.get_BoundingBox(view).Max).Z;

            // Получаем координаты точек max и min рамки подрезки вида в координатах проекта
            var viewMax = _viewPointsAnalyzer.ProjectPointToViewFront(
                view.CropBox.Transform.OfPoint(view.CropBox.Max));
            var viewMin = _viewPointsAnalyzer.ProjectPointToViewFront(
                view.CropBox.Transform.OfPoint(view.CropBox.Min));

            // Ищем толщину последней плиты
            // В качестве значения по умолчанию задаем 200 мм как наиболее часто встречающуюся толщину плиты
            var lastFloorDepth = UnitUtilsHelper.ConvertToInternalValue(200);
            var lastFloor = GetLastFloor();
            if(lastFloor != null) {
                lastFloorDepth = lastFloor.GetParamValue<double>(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM);
            }

            // Получаем точки со смещением относительно граничных точек опалубки для размещения аннотаций
            // Правая нижняя точка
            var point1 = _viewPointsAnalyzer
                        .GetPointByDirection(new XYZ(viewMax.X, viewMax.Y, pylonMaxZ),
                                             DirectionType.LeftBottom, _breakLinesOffsetX, 0);
            // Правая верхняя точка
            var point2 = _viewPointsAnalyzer
                .GetPointByDirection(point1,
                                     DirectionType.Top, 0, lastFloorDepth);
            // Левая нижняя точка
            var point4 = _viewPointsAnalyzer
                .GetPointByDirection(new XYZ(viewMin.X, viewMin.Y, pylonMaxZ),
                                     DirectionType.RightBottom, _breakLinesOffsetX, 0);
            // Левая верхняя точка
            var point3 = _viewPointsAnalyzer
                .GetPointByDirection(point4,
                                     DirectionType.Top, 0, lastFloorDepth);

            // Линии обрыва - это 2D-аннотационные семейства на основе линии
            var line1 = Line.CreateBound(point1, point2);
            var line2 = Line.CreateBound(point3, point4);
            var breakLine1 = Repository.Document.Create.NewFamilyInstance(line1, _breakLineSymbol, view);
            var breakLine2 = Repository.Document.Create.NewFamilyInstance(line2, _breakLineSymbol, view);

            TrySetBreakLineOffsets(breakLine1);
            TrySetBreakLineOffsets(breakLine2);
        } catch(Exception) { }
    }

    /// <summary>
    /// Создаем линии обрыва между опалубочными элементами, если их несколько
    /// </summary>
    internal void TryCreateMiddleBreakLines(bool isForPerpView) {
        if(SheetInfo.HostElems.Count == 1 || _breakLineSymbol is null) {
            return;
        }
        var view = ViewOfPylon.ViewElement;
        try {
            // Определяем отступы от пилона по горизонтали
            double horizOriginOffset = isForPerpView 
                                            ? SheetInfo.ElemsInfo.HostWidth * 0.5 + _breakLinesOffsetY
                                            : SheetInfo.ElemsInfo.HostLength * 0.5 + _breakLinesOffsetY;

            var origin = SheetInfo.ElemsInfo.HostOrigin;
            // Получаем спроецированную на плоскость вида левую граничную точку пилона
            var pylonLeftMinPoint = _viewPointsAnalyzer.GetPointByDirection(origin, DirectionType.Left,
                                                                            horizOriginOffset, 0);
            pylonLeftMinPoint = _viewPointsAnalyzer.ProjectPointToViewFront(pylonLeftMinPoint);
            // Получаем спроецированную на плоскость вида правую граничную точку пилона
            var pylonRightMinPoint = _viewPointsAnalyzer.GetPointByDirection(origin, DirectionType.Right,
                                                                             horizOriginOffset, 0);
            pylonRightMinPoint = _viewPointsAnalyzer.ProjectPointToViewFront(pylonRightMinPoint);

            var previousElement = SheetInfo.HostElems.First();
            for(int i = 1; i < SheetInfo.HostElems.Count; i++) {
                var currentElement = SheetInfo.HostElems[i];

                // Получаем спроецированные на плоскость вида граничные точки вида
                var currentBbMinZ = currentElement.get_BoundingBox(view).Min.Z;
                var previousBbMaxZ = previousElement.get_BoundingBox(view).Max.Z;

                // Получаем точки со смещением относительно граничных точек опалубки для размещения аннотаций
                var point1 = new XYZ(pylonLeftMinPoint.X, pylonLeftMinPoint.Y, currentBbMinZ);
                var point2 = new XYZ(pylonLeftMinPoint.X, pylonLeftMinPoint.Y, previousBbMaxZ);
                var point3 = new XYZ(pylonRightMinPoint.X, pylonRightMinPoint.Y, previousBbMaxZ);
                var point4 = new XYZ(pylonRightMinPoint.X, pylonRightMinPoint.Y, currentBbMinZ);
                
                // Линии обрыва - это 2D-аннотационные семейства на основе линии
                var line1 = Line.CreateBound(point1, point2);
                var line2 = Line.CreateBound(point3, point4);
                var breakLine1 = Repository.Document.Create.NewFamilyInstance(line1, _breakLineSymbol, view);
                var breakLine2 = Repository.Document.Create.NewFamilyInstance(line2, _breakLineSymbol, view);

                TrySetBreakLineOffsets(breakLine1);
                TrySetBreakLineOffsets(breakLine2);

                previousElement = currentElement;
            }
        } catch(Exception) { }
    }

    /// <summary>
    /// Задает отступы области маскировки у линии обрыва
    /// </summary>
    private void TrySetBreakLineOffsets(Element breakLine) {
        if(breakLine.IsExistsParam(_breakLineDepthParamName)) {
            breakLine.SetParamValue(_breakLineDepthParamName, 
                                    UnitUtilsHelper.ConvertToInternalValue(_breakLineDepthParamValue));
        }
        if(breakLine.IsExistsParam(_breakLineLengthLeftParamName)) {
            breakLine.SetParamValue(_breakLineLengthLeftParamName,
                                    UnitUtilsHelper.ConvertToInternalValue(_breakLineLengthLeftParamValue));
        }
        if(breakLine.IsExistsParam(_breakLineLengthRightParamName)) {
            breakLine.SetParamValue(_breakLineLengthRightParamName, 
                                    UnitUtilsHelper.ConvertToInternalValue(_breakLineLengthRightParamValue));
        }
    }

    private Element GetLastFloor() {
        var lastPylon = SheetInfo.HostElems.Last();
        var bbox = lastPylon.get_BoundingBox(null);

        // Готовим фильтр для сбор плит в области вокруг верхней точки пилона
        var outline = new Outline(
            bbox.Max - new XYZ(10, 10, 5),
            bbox.Max + new XYZ(10, 10, 5)
        );
        var filter = new BoundingBoxIntersectsFilter(outline);

        return new FilteredElementCollector(Repository.Document)
            .OfCategory(BuiltInCategory.OST_Floors)
            .WherePasses(filter)
            .WhereElementIsNotElementType()
            .FirstOrDefault();
    }

    /// <summary>
    /// Создание аннотации рабочего шва бетонирования
    /// </summary>
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
