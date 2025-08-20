using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

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
            var dimensionLineTop = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Top, 1);
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
                                                                            DimensionOffsetType.Bottom, 1);
            var refArrayBottom = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', 
                                                                       ["низ", "торец"]);
            Repository.Document.Create.NewDimension(ViewOfPylon.ViewElement, dimensionLineBottom, refArrayBottom, 
                                                    ViewModel.SelectedDimensionType);
        } catch(Exception) { }
    }

    /// <summary>
    /// Создание размерной цепочки по пластинам сбоку
    /// </summary>
    internal void TryCreatePlateDimensions(FamilyInstance skeletonParentRebar,
                                           DimensionOffsetType dimensionOffsetType, 
                                           DimensionBaseService dimensionBaseService) {
        var view = ViewOfPylon.ViewElement;
        try {

            List<Element> platesArray = ViewModel.RebarFinder.GetSimpleRebars(view, SheetInfo.ProjectSection, 2001);
            ReferenceArray refArraySide;
            if(SheetInfo.RebarInfo.AllRebarAreL) {
                // #1_горизонт_Г-стержень
                refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', 
                                                                     ["горизонт", "Г-стержень"]);
            } else {
                // #1_горизонт_выпуск
                refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', 
                                                                     ["горизонт", "выпуск"]);
            }
            // #_1_горизонт_край_низ
            refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', 
                                                                 ["горизонт", "край", "низ"], oldRefArray: refArraySide);
            var dimensionLineLeftFirst = dimensionBaseService.GetDimensionLine(skeletonParentRebar, 
                                                                               dimensionOffsetType, 1.3);
            var dimensionRebarSideFirst = Repository.Document.Create.NewDimension(view, dimensionLineLeftFirst, 
                                                                                  refArraySide, 
                                                                                  ViewModel.SelectedDimensionType);
            double lengthTemp = 0.0;
            Element neededPlates = default;
            foreach(var plates in platesArray) {
                var length = plates.GetParamValue<double>("мод_Длина");
                if(length > lengthTemp) {
                    lengthTemp = length;
                    neededPlates = plates;
                }
            }
            var viewOptions = new Options {
                View = view,
                ComputeReferences = true,
                IncludeNonVisibleObjects = false
            };
            var plateRefs = neededPlates.get_Geometry(viewOptions)?
                .OfType<GeometryInstance>()
                .SelectMany(ge => ge.GetSymbolGeometry())
                .OfType<Solid>()
                .Where(solid => solid?.Volume > 0)
                .SelectMany(solid => solid.Faces.OfType<PlanarFace>())
                .Where(face => Math.Abs(face.FaceNormal.Z + 1) < 0.001)
                .ToList();
            foreach(var plateRef in plateRefs) {
                refArraySide.Append(plateRef.Reference);
            }
            var dimensionLineLeftSecond = dimensionBaseService.GetDimensionLine(skeletonParentRebar, 
                                                                                dimensionOffsetType, 0.7);
            Repository.Document.Create.NewDimension(view, dimensionLineLeftSecond,
                                                    refArraySide, ViewModel.SelectedDimensionType);
        } catch(Exception) { }
    }


    /// <summary>
    /// Создание размера сбоку между низом арматурного каркаса и Г-образным стержнем
    /// </summary>
    internal void TryCreateLRebarDimension(FamilyInstance skeletonParentRebar, DimensionOffsetType dimensionOffsetType, 
                                           DimensionBaseService dimensionBaseService) {
        try {
            // #1_горизонт_Г-стержень
            var refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                     ["горизонт", "Г-стержень"]);
            // #_1_горизонт_край_низ
            refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', 
                                                                 ["горизонт", "край", "низ"],
                                                                 oldRefArray: refArraySide);
            var dimensionLineLeftFirst = dimensionBaseService.GetDimensionLine(skeletonParentRebar, 
                                                                               dimensionOffsetType, 1.3);
            var dimensionRebarSideFirst = Repository.Document.Create.NewDimension(ViewOfPylon.ViewElement,
                                                                                  dimensionLineLeftFirst, refArraySide,
                                                                                  ViewModel.SelectedDimensionType);
        } catch(Exception) { }
    }

    /// <summary>
    /// Создание размера сбоку между низом арматурного каркаса и Г-образным стержнем
    /// </summary>
    internal void TryCreateLRebarDimension(FamilyInstance skeletonParentRebar, DimensionBaseService dimensionBaseService) {
        try {
            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            if(!SheetInfo.RebarInfo.HasLRebar) { return; }
            // Г-образный стержень
            var lRebar = ViewModel.RebarFinder.GetSimpleRebars(ViewOfPylon.ViewElement, SheetInfo.ProjectSection, 1101)
                                              .FirstOrDefault();
            var dimensionLine = dimensionBaseService.GetDimensionLine(lRebar, DimensionOffsetType.Top, 0.5);
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
            var dimensionLineTop = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Top, -2.3);

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
