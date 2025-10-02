using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewRebarMarkServices;
internal class GeneralViewRebarMarkService {
    private readonly ViewPointsAnalyzer _viewPointsAnalyzer;
    private readonly TagCreationService _annotationService;
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
        _annotationService = new TagCreationService(ViewOfPylon);

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

#if REVIT_2022_OR_GREATER
            var hostOrigin = SheetInfo.ElemsInfo.HostOrigin;
            var hostOriginProjected = _viewPointsAnalyzer.ProjectPointToViewFront(hostOrigin);

            var pointProjected = _viewPointsAnalyzer.ProjectPointToViewFront(point);
            pointProjected = _viewPointsAnalyzer.ProjectPointToHorizontalPlane(hostOriginProjected, pointProjected);
            var dist = pointProjected.DistanceTo(hostOriginProjected);

            var hostWidth = SheetInfo.ElemsInfo.HostWidth;
            if(dist > hostWidth) {
                point = _viewPointsAnalyzer.GetPointByDirection(
                    new XYZ(hostOriginProjected.X, hostOriginProjected.Y, point.Z), directionType, hostWidth, 0);
            }
#endif
            // Создаем марку арматуры
            var tagOption = new TagOption() {
                BodyPoint = point,
                TagSymbol = _tagSymbolWithStep
            };
            var tag = _annotationService.CreateRebarTag(tagOption, verticalBar);
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

            List<IndependentTag> tags = new List<IndependentTag>();
            List<IndependentTag> tagsToDelete = new List<IndependentTag>();
            foreach(var simpleClamp in simpleClamps) {
                var clampPoint = _viewPointsAnalyzer.GetTransformedPoint(simpleClamp, true);
                IndependentTag tag = default;
                if(!isFrontView || clampPoint.X > pointForCompare.X) {
                    tag = TryCreateClampMark(simpleClamp, DirectionType.RightTop, isFrontView);
                } else {
                    tag = TryCreateClampMark(simpleClamp, DirectionType.LeftTop, isFrontView);
                }
                tags.Add(tag);
            }
            // Удаление марок, если они стоят слишком близко. Это возможно при ситуации, когда вид на пилон сбоку и
            // в сечении два хомута, соответственно две марки, а достаточно одной
            double maxDistance = 4;
            for(int i = 0; i < tags.Count; i++) {
                IndependentTag currentTag = tags[i];
                XYZ currentTagPosition = currentTag.TagHeadPosition;

                for(int j = i + 1; j < tags.Count; j++) {
                    IndependentTag tagForCompare = tags[j];
                    XYZ tagForComparePosition = tagForCompare.TagHeadPosition;

                    double distance = currentTagPosition.DistanceTo(tagForComparePosition);
                    if(distance < maxDistance) {
                        // Добавляем одну из марок в список для удаления
                        if(!tagsToDelete.Contains(tagForCompare)) {
                            tagsToDelete.Add(tagForCompare);
                        }
                    }
                }
            }
            foreach(var tag in tagsToDelete) {
                Repository.Document.Delete(tag.Id);
            }
        } catch(Exception) { }
    }

    /// <summary>
    /// Создает марку хомута на основном виде опалубки
    /// </summary>
    private IndependentTag TryCreateClampMark(Element simpleClamp, DirectionType directionType, bool isFrontView) {
        try {
            var xOffset = isFrontView ? 2.4 : 1;
            // Получаем точку в которую нужно поставить аннотацию
            var annotPoint = _viewPointsAnalyzer.GetPointByDirection(simpleClamp, directionType, 0, 0, true);
            // Корректируем положение точки, куда будет установлена марка (текст)
            annotPoint = _viewPointsAnalyzer.GetPointByDirection(annotPoint, directionType, xOffset, 0.3);

            // Создаем марку арматуры
            var tagOption = new TagOption() { BodyPoint = annotPoint, TagSymbol = _tagSymbolWithStep };
            var clampTag = _annotationService.CreateRebarTag(tagOption, simpleClamp);
            clampTag.LeaderEndCondition = LeaderEndCondition.Free;
            return clampTag;
        } catch(Exception) { }
        return null;
    }
}
