using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewFormMarkServices;
internal class GeneralViewMarkService {
    private readonly FamilySymbol _tagSkeletonSymbol;
    private readonly FamilySymbol _universalTagType;
    private readonly FamilySymbol _concretingSeamSymbol;

    private readonly DimensionBaseService _dimensionBaseService;
    private readonly ViewPointsAnalyzerService _viewPointsAnalyzer;
    private readonly TagCreationService _annotationService;

    internal GeneralViewMarkService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                    PylonView pylonView, DimensionBaseService dimensionBaseService) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        ViewOfPylon = pylonView;
        _dimensionBaseService = dimensionBaseService;

        _viewPointsAnalyzer = new ViewPointsAnalyzerService(ViewOfPylon);
        _annotationService = new TagCreationService(ViewOfPylon);

        // Находим типоразмер марки несущей арматуры для обозначения марки изделия
        _tagSkeletonSymbol = mvm.SelectedSkeletonTagType;

        // Находим типоразмер марки несущей арматуры для обозначения позиции, диаметра и комментариев арматуры
        // Находим типоразмер типовой аннотации для метки ГОСТа сварки
        _universalTagType = mvm.SelectedUniversalTagType;

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
            var tagOption = new TagOption() { BodyPoint = point, TagSymbol = _tagSkeletonSymbol };
            var rightTag = _annotationService.CreateRebarTag(tagOption, rightVerticalBar);
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
