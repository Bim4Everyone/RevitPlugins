using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.Services;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewFormMarkServices;
internal class GeneralViewMarkService {
    private readonly Document _doc;
    private readonly PylonSheetInfo _sheetInfo;
    private readonly PylonView _viewOfPylon;

    private readonly FamilySymbol _tagSkeletonSymbol;
    private readonly FamilySymbol _universalTagType;
    private readonly FamilySymbol _concretingSeamSymbol;
    private readonly SpotDimensionType _spotDimensionType;

    private readonly DimensionBaseService _dimensionBaseService;
    private readonly ViewPointsAnalyzerService _viewPointsAnalyzer;
    private readonly TagCreationService _annotationService;

    internal GeneralViewMarkService(CreationSettings settings, Document document, PylonSheetInfo pylonSheetInfo,
                                    PylonView pylonView, DimensionBaseService dimensionBaseService) {
        _doc = document;
        _sheetInfo = pylonSheetInfo;
        _viewOfPylon = pylonView;
        _dimensionBaseService = dimensionBaseService;

        _viewPointsAnalyzer = new ViewPointsAnalyzerService(_viewOfPylon);
        _annotationService = new TagCreationService(_viewOfPylon);

        // Находим типоразмер марки несущей арматуры для обозначения марки изделия
        _tagSkeletonSymbol = settings.AnnotationSettings.SelectedSkeletonTagType;

        // Находим типоразмер марки несущей арматуры для обозначения позиции, диаметра и комментариев арматуры
        // Находим типоразмер типовой аннотации для метки ГОСТа сварки
        _universalTagType = settings.AnnotationSettings.SelectedUniversalTagType;

        // Находим типоразмер аннотации рабочего шва бетонирования
        _concretingSeamSymbol = settings.AnnotationSettings.SelectedConcretingJointType;

        // Находим типоразмер аннотации высотной отметки
        _spotDimensionType = settings.AnnotationSettings.SelectedSpotDimensionType;
    }

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
                    var spotElevation = _doc.Create.NewSpotElevation(
                        _viewOfPylon.ViewElement,
                        reference,
                        location,
                        location,
                        location,
                        location,
                        false);
                    spotElevation.ChangeTypeId(_spotDimensionType.Id);
                }
            }
        } catch(Exception) { }
    }

    /// <summary>
    /// Создает марку арматурного каркаса на основном виде опалубки
    /// </summary>
    internal void TryCreateSkeletonMark(bool isForPerpView) {
        var simpleRebars = _sheetInfo.RebarInfo.SimpleVerticalRebars;
        if(simpleRebars.Count == 0) { return; }
        var simpleRebarsInView = _sheetInfo.RebarFinder.GetRebarsFromView(simpleRebars, _viewOfPylon.ViewElement);
        if(simpleRebarsInView.Count == 0) { return; }

        try {
            // Получаем референс-элемент
            var rightVerticalBar = _viewPointsAnalyzer.GetElementByDirection(simpleRebarsInView, DirectionType.Right,
                                                                             false);
            // Получаем точку в которую нужно поставить аннотацию
            var point = _viewPointsAnalyzer.GetPointByDirection(rightVerticalBar, DirectionType.Right, 0, 0, true);
            // Корректируем положение точки, куда будет установлена марка (текст)
            // Смещение по верикали привязываем к высоте армирования, т.к. нужно соблюсти баланс между 
            // одноэтажным и двухэтажным пилоном
            point = _viewPointsAnalyzer.GetPointByDirection(
                point,
                DirectionType.RightBottom,
                0.8,
                _sheetInfo.ElemsInfo.ElemsBoundingBoxHeight / 8);
            // Корректируем положение точки, если она слишком удалена от пилона (из-за семейства Гэшки)
#if REVIT_2022_OR_GREATER
            if(isForPerpView) {
                var hostOrigin = _sheetInfo.ElemsInfo.HostOrigin;
                var hostOriginProjected = _viewPointsAnalyzer.ProjectPointToViewFront(hostOrigin);

                var pointProjected = _viewPointsAnalyzer.ProjectPointToViewFront(point);
                pointProjected = _viewPointsAnalyzer.ProjectPointToHorizontalPlane(hostOriginProjected, pointProjected);
                var dist = pointProjected.DistanceTo(hostOriginProjected);

                var hostWidth = _sheetInfo.ElemsInfo.HostWidth;
                if(dist > hostWidth) {
                    point = _viewPointsAnalyzer.GetPointByDirection(
                        new XYZ(hostOriginProjected.X, hostOriginProjected.Y, point.Z), DirectionType.Right, hostWidth, 0);
                }
            }
#endif
            // Создаем марку арматуры
            var tagOption = new TagOption() { BodyPoint = point, TagSymbol = _tagSkeletonSymbol };
            var rightTag = _annotationService.CreateRebarTag(tagOption, rightVerticalBar);
            rightTag.LeaderEndCondition = LeaderEndCondition.Free;

#if REVIT_2022_OR_GREATER
            var rightVerticalBarRef = new Reference(rightVerticalBar);
            var tagLeaderEnd = rightTag.GetLeaderEnd(rightVerticalBarRef);

            // Смещение по верикали привязываем к высоте армирования, т.к. нужно соблюсти баланс между 
            // одноэтажным и двухэтажным пилоном
            tagLeaderEnd = _viewPointsAnalyzer.GetPointByDirection(
                tagLeaderEnd,
                DirectionType.Bottom,
                0,
                _sheetInfo.ElemsInfo.ElemsBoundingBoxHeight / 9);
            rightTag.SetLeaderEnd(rightVerticalBarRef, tagLeaderEnd);
#endif
        } catch(Exception) { }
    }

    internal void TryCreateAdditionalMark(bool isForPerpView) {
        try {
            // Если в пилоне есть семейство при помощи которого армируются пилоны паркинга, то выпусков снизу не будет
            // А это значит, что и данная марка не нужна
            if(_sheetInfo.RebarInfo.SkeletonParentRebarForParking) { return; }

            var view = _viewOfPylon.ViewElement;
            // Определяем отступ от пилона по горизонтали
            double horizOriginOffset = isForPerpView
                                            ? _sheetInfo.ElemsInfo.HostWidth * 0.5
                                            : _sheetInfo.ElemsInfo.HostLength * 0.5;
            var origin = _sheetInfo.ElemsInfo.HostOrigin;
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
            var tagOption = new TagOption() {
                BodyPoint = annotPoint,
                TagSymbol = _universalTagType,
                TagLength = UnitUtilsHelper.ConvertToInternalValue(40),
                TopText = "Арматурные выпуски",
                BottomText = "нижнего пилона"
            };
            _annotationService.CreateUniversalTag(tagOption, leaderPoint);
        } catch(Exception) { }
    }

    /// <summary>
    /// Создание аннотации рабочего шва бетонирования
    /// </summary>
    internal void TryCreateConcretingSeams() {
        var view = _viewOfPylon.ViewElement;
        if(_concretingSeamSymbol is null) { return; }

        try {
            foreach(var pylon in _sheetInfo.HostElems) {
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
                _doc.Create.NewFamilyInstance(line1, _concretingSeamSymbol, view);
                _doc.Create.NewFamilyInstance(line2, _concretingSeamSymbol, view);
            }
        } catch(Exception) { }
    }
}
