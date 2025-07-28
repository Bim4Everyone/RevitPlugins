using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices;
internal class GeneralViewMarkService {
    private readonly int _formNumberForClampsMax = 1599;
    private readonly int _formNumberForClampsMin = 1500;

    private readonly FamilySymbol _tagSkeletonSymbol;
    private readonly FamilySymbol _tagSymbolWithoutSerif;

    private readonly ViewPointsAnalyzer _viewPointsAnalyzer;
    private readonly AnnotationService _annotationService;

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
}
