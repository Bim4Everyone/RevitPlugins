using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
internal class GeneralViewRebarPerpDimensionService {
    private readonly ViewPointsAnalyzer _viewPointsAnalyzer;
    internal GeneralViewRebarPerpDimensionService(MainViewModel mvm, RevitRepository repository, 
                                                  PylonSheetInfo pylonSheetInfo, PylonView pylonView) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        ViewOfPylon = pylonView;
        _viewPointsAnalyzer = new ViewPointsAnalyzer(ViewOfPylon);
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }
    internal PylonView ViewOfPylon { get; set; }



    /// <summary>
    /// Создание размерной цепочки по крайним вертикальным стержням арматурного каркаса сверху
    /// </summary>
    internal void TryCreateTopEdgeRebarDimensions(FamilyInstance skeletonParentRebar, 
                                               DimensionBaseService dimensionBaseService) {
        try {
            // Если хотя бы один из стержней - Г-образный,тогда нет смысле ставить этот размер
            // Ставится только когда обе бутылки
            if(SheetInfo.RebarInfo.HasLRebar) { return; }
            var dimensionLineTop = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DirectionType.Top, 1);
            var refArrayTop = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["верх", "торец"]);
            Repository.Document.Create.NewDimension(ViewOfPylon.ViewElement, dimensionLineTop, refArrayTop, 
                                                    ViewModel.SelectedDimensionType);
        } catch(Exception) { }
    }

    /// <summary>
    /// Создание размерной цепочки по крайним вертикальным стержням арматурного каркаса снизу
    /// </summary>
    internal void TryCreateBottomEdgeRebarDimensions(FamilyInstance skeletonParentRebar, 
                                                  DimensionBaseService dimensionBaseService) {
        try {
            var dimensionLineBottom = dimensionBaseService.GetDimensionLine(skeletonParentRebar,
                                                                            DirectionType.Bottom, 1);
            var refArrayBottom = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', 
                                                                       ["низ", "торец"]);
            Repository.Document.Create.NewDimension(ViewOfPylon.ViewElement, dimensionLineBottom, refArrayBottom, 
                                                    ViewModel.SelectedDimensionType);
        } catch(Exception) { }
    }

    /// <summary>
    /// Создание размера сбоку между низом арматурного каркаса и Г-образным стержнем
    /// </summary>
    internal void TryCreateVertLRebarDimension(FamilyInstance skeletonParentRebar, 
                                               DimensionBaseService dimensionBaseService) {
        try {
            if(!SheetInfo.RebarInfo.HasLRebar) { return; }
            var refArraySideBottom = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                           ["горизонт", "край", "низ"]);
            var refArraySideRight = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                          ["горизонт", "Г-стержень"],
                                                                             oldRefArray: refArraySideBottom);
            var dimensionLineRight = dimensionBaseService.GetDimensionLine(skeletonParentRebar,
                                                                           DirectionType.Right, 0.7);
            var dimensionRebarSideRight =
                Repository.Document.Create.NewDimension(ViewOfPylon.ViewElement, dimensionLineRight,
                                                        refArraySideRight, ViewModel.SelectedDimensionType);
        } catch(Exception) { }
    }

    /// <summary>
    /// Создание размера сверху по Г-образному стержню
    /// </summary>
    internal void TryCreateHorizLRebarDimension(FamilyInstance skeletonParentRebar, 
                                                DimensionBaseService dimensionBaseService) {
        try {
            if(!SheetInfo.RebarInfo.HasLRebar) { return; }
            // Г-образный стержень
            var lRebar = ViewModel.RebarFinder.GetSimpleRebars(ViewOfPylon.ViewElement, SheetInfo.ProjectSection, 1101)
                                              .FirstOrDefault();
            var dimensionLine = dimensionBaseService.GetDimensionLine(lRebar, DirectionType.Top, 0.5);
            //"#1_торец_Г_нутрь"
            //"#1_торец_Г_край"
            if(SheetInfo.RebarInfo.AllRebarAreL) {
                CreateLRebarDimension(skeletonParentRebar, dimensionLine, dimensionBaseService, ["#1_торец_Г"]);
                CreateLRebarDimension(skeletonParentRebar, dimensionLine, dimensionBaseService, ["#2_торец_Г"]);
            } else if(SheetInfo.RebarInfo.HasLRebar) {
                if(ViewModel.RebarFinder.DirectionHasLRebar(ViewOfPylon.ViewElement, SheetInfo.ProjectSection, DirectionType.Right)
                    && SheetInfo.RebarInfo.SecondLRebarParamValue) {
                    CreateLRebarDimension(skeletonParentRebar, dimensionLine, dimensionBaseService, ["#2_торец_Г"]);
                } else {
                    CreateLRebarDimension(skeletonParentRebar, dimensionLine, dimensionBaseService, ["#1_торец_Г"]);
                }
            }
        } catch(Exception) { }
    }

    private void CreateLRebarDimension(FamilyInstance skeletonParentRebar, Line dimensionLine,
                                       DimensionBaseService dimensionBaseService,
                                       List<string> importantRefNameParts, 
                                       List<string> unimportantRefNameParts = null) {
        var refArray = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', importantRefNameParts,
                                                             unimportantRefNameParts);
        Repository.Document.Create.NewDimension(ViewOfPylon.ViewElement, dimensionLine, refArray,
                                                ViewModel.SelectedDimensionType);
    }

    /// <summary>
    /// Создание размеров по изгибам вертикальных стержней-бутылок
    /// </summary>
    internal void TryCreateAdditionalDimensions(FamilyInstance skeletonParentRebar, 
                                                DimensionBaseService dimensionBaseService) {
        try {
            var doc = Repository.Document;
            var view = ViewOfPylon.ViewElement;
            // Создаем размеры по изгибам бутылок
            var dimensionLineTop = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DirectionType.Top, -2.3);

            var refArrayTop1 = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["1_торец"], ["Г"]);
            var dimensionTop1 = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop1,
                                                        ViewModel.SelectedDimensionType);

            var refArrayTop2 = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["2_торец"], ["Г"]);
            var dimensionTop2 = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop2,
                                                        ViewModel.SelectedDimensionType);
            // Смещаем текст размера для корректного отображения
            var rightDirection = view.RightDirection;
            var rightDirectionInversed = rightDirection.Negate();
            double offsetX = 0.3;

            var offsetLeft = new XYZ(rightDirection.X * offsetX, rightDirection.Y * offsetX, rightDirection.Z);
            var offsetRight = new XYZ(rightDirectionInversed.X * offsetX, 
                                      rightDirectionInversed.Y * offsetX, 
                                      rightDirectionInversed.Z);
            var textPosition1 = dimensionTop1.TextPosition;
            var textPosition1Transformed = _viewPointsAnalyzer.GetTransformedPoint(textPosition1);

            var textPosition2 = dimensionTop2.TextPosition;
            var textPosition2Transformed = _viewPointsAnalyzer.GetTransformedPoint(textPosition2);
            if(textPosition1Transformed.X > textPosition2Transformed.X) {
                dimensionTop1.TextPosition += offsetLeft;
                dimensionTop2.TextPosition += offsetRight;
            } else {
                dimensionTop1.TextPosition += offsetRight;
                dimensionTop2.TextPosition += offsetLeft;
            }

            // В случае, если размер имеет нулевое значение, то удаляем его
            if(dimensionTop1.ValueString == "0") {
                doc.Delete(dimensionTop1.Id);
                dimensionTop1 = null;
            }
            if(dimensionTop2.ValueString == "0") {
                doc.Delete(dimensionTop2.Id);
                dimensionTop2 = null;
            }
        } catch(Exception) { }
    }
}
