using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices;
internal class GeneralViewMarkService {
    private readonly FamilySymbol _tagSkeletonSymbol;
    internal GeneralViewMarkService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                    PylonView pylonView) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        ViewOfPylon = pylonView;

        // Находим типоразмер марки несущей арматуры для обозначения марки изделия
        _tagSkeletonSymbol = Repository.FindSymbol(BuiltInCategory.OST_RebarTags, "Изделие_Марка - Полка 30");
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
    internal void CreateSkeletonMark(List<Element> simpleRebars) {
        if(simpleRebars.Count == 0) { return; }
        var viewPointsAnalyzer = new ViewPointsAnalyzer(ViewOfPylon);
        var annotationService = new AnnotationService(ViewOfPylon);

        // Получаем референс-элемент
        Element rightVerticalBar = viewPointsAnalyzer.GetElementByDirection(simpleRebars, DirectionType.Right,
                                                                            false);
        // Получаем точку в которую нужно поставить аннотацию
        var pointRight= viewPointsAnalyzer.GetPointByDirection(rightVerticalBar, DirectionType.Right,
                                                               0, 0, true);
        // Корректируем положение точки, куда будет установлена марка (текст)
        pointRight = viewPointsAnalyzer.GetPointByDirection(pointRight, DirectionType.RightBottom, 1.5, 3.5);
        // Создаем марку арматуры
        var rightTag = annotationService.CreateRebarTag(pointRight, _tagSkeletonSymbol, rightVerticalBar);

#if REVIT_2022_OR_GREATER
        rightTag.LeaderEndCondition = LeaderEndCondition.Free;
        var rightVerticalBarRef = new Reference(rightVerticalBar);

        var tagLeaderEnd = rightTag.GetLeaderEnd(rightVerticalBarRef);
        tagLeaderEnd = viewPointsAnalyzer.GetPointByDirection(tagLeaderEnd, DirectionType.Bottom, 0, 3);
        rightTag.SetLeaderEnd(rightVerticalBarRef, tagLeaderEnd);
#endif
    }
}
