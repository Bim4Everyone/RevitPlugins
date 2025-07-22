using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitPylonDocumentation.ViewModels;

using Grid = Autodesk.Revit.DB.Grid;
using Line = Autodesk.Revit.DB.Line;

namespace RevitPylonDocumentation.Models.PylonSheetNView;
public class PylonViewDimensionCreator {
    internal PylonViewDimensionCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        View = pylonView;
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }
    internal PylonView View { get; set; }


    //public void TryCreateGeneralViewDimensions() {
        
    //}


    

    

    public void TryCreateTransverseViewFirstDimensions() {
        View view = SheetInfo.TransverseViewFirst.ViewElement;
        TryCreateTransverseViewDimensions(view, false);
    }

    public void TryCreateTransverseViewSecondDimensions() {
        View view = SheetInfo.TransverseViewSecond.ViewElement;
        TryCreateTransverseViewDimensions(view, false);
    }

    public void TryCreateTransverseViewThirdDimensions() {
        View view = SheetInfo.TransverseViewThird.ViewElement;
        TryCreateTransverseViewDimensions(view, true);
    }


    public void TryCreateGeneralRebarViewDimensions() {
        
    }



    




    public void TryCreateGeneralRebarPerpendicularViewDimensions() {
        
    }


    


    public void TryCreateGeneralRebarPerpendicularViewAdditionalDimensions() {
        var doc = Repository.Document;
        var view = SheetInfo.GeneralViewPerpendicularRebar.ViewElement;
        var dimensionBaseService = new DimensionBaseService(view, ViewModel.ParamValService);

        try {
            var rebarFinder = ViewModel.RebarFinder;
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }

            Line dimensionLineTop = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Top, -1);

            ReferenceArray refArrayTop_1 = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "1_торец" });
            Dimension dimensionTop_1 = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop_1, ViewModel.SelectedDimensionType);
            if(dimensionTop_1.Value == 0) {
                doc.Delete(dimensionTop_1.Id);
            }

            ReferenceArray refArrayTop_2 = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "2_торец" });
            Dimension dimensionTop_2 = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop_2, ViewModel.SelectedDimensionType);
            if(dimensionTop_2.Value == 0) {
                doc.Delete(dimensionTop_2.Id);
            }

            //// Смещение выноски вправо
            //var rightDirection = GetViewDirections(view).RightDirection;
            //// .Multiply(offsetCoefficient)

            //var dimensionPoint_1 = dimensionBottom_1.LeaderEndPosition;
            //var dimensionPoint_2 = dimensionBottom_2.LeaderEndPosition;

            //dimensionPoint_1 = new XYZ(dimensionPoint_1.X, dimensionPoint_1.Y, 0);
            //dimensionPoint_2 = new XYZ(dimensionPoint_2.X, dimensionPoint_2.Y, 0);

            //var viewMin = view.CropBox.Min;
            //viewMin = new XYZ(viewMin.X, viewMin.Y, 0);

            //if(dimensionPoint_1.DistanceTo(viewMin) < dimensionPoint_2.DistanceTo(viewMin)) {
            //    dimensionBottom_1.LeaderEndPosition = dimensionPoint_1 + rightDirection;
            //    dimensionBottom_2.LeaderEndPosition = dimensionPoint_2 - rightDirection;
            //} else {
            //    dimensionBottom_1.LeaderEndPosition = dimensionPoint_1 - rightDirection;
            //    dimensionBottom_2.LeaderEndPosition = dimensionPoint_2 + rightDirection;
            //}
        } catch(Exception) { }
    }


    public void TryCreateTransverseRebarViewFirstDimensions() {
        var doc = Repository.Document;
        var view = SheetInfo.TransverseViewFirstRebar.ViewElement;
        var dimensionBaseService = new DimensionBaseService(view, ViewModel.ParamValService);

        try {
            var rebarFinder = ViewModel.RebarFinder;
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }

            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            Line dimensionLineBottom = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Bottom, 0.5);
            ReferenceArray refArrayBottom = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "низ", "фронт" });
            Dimension dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom, ViewModel.SelectedDimensionType);

            Line dimensionLineBottomEdge = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Bottom, 1);
            ReferenceArray refArrayBottomEdge = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "низ", "фронт", "край" });
            Dimension dimensionBottomEdge = doc.Create.NewDimension(view, dimensionLineBottomEdge, refArrayBottomEdge, ViewModel.SelectedDimensionType);

            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            // Размер по ТОРЦУ армирование (положение справа 2)
            Line dimensionLineRightSecond = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Right, 0.5);
            // Добавляем ссылки на арматурные стержни
            ReferenceArray refArrayRebarSide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                new List<string>() { "низ", "торец", "край" });
            Dimension dimensionFormworkRebarSide = doc.Create.NewDimension(view, dimensionLineRightSecond,
                                                                              refArrayRebarSide, ViewModel.SelectedDimensionType);
        } catch(Exception) { }
    }

    public void TryCreateTransverseRebarViewSecondDimensions() {
        var doc = Repository.Document;
        var view = SheetInfo.TransverseViewSecondRebar.ViewElement;
        var dimensionBaseService = new DimensionBaseService(view, ViewModel.ParamValService);

        try {
            var rebarFinder = ViewModel.RebarFinder;
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }

            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            Line dimensionLineTop = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Top, 0.5);
            ReferenceArray refArrayTop = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "верх", "фронт" });
            Dimension dimensionTop = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop, ViewModel.SelectedDimensionType);

            Line dimensionLineTopEdge = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Top, 1);
            ReferenceArray refArrayTopEdge = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                              new List<string>() { "верх", "фронт", "край" });
            Dimension dimensionTopEdge = doc.Create.NewDimension(view, dimensionLineTopEdge, refArrayTopEdge, ViewModel.SelectedDimensionType);

            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            // Размер по ТОРЦУ армирование (положение справа 2)
            Line dimensionLineRightSecond = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Right, 0.5);
            // Добавляем ссылки на арматурные стержни
            ReferenceArray refArrayRebarSide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                new List<string>() { "низ", "торец", "край" });
            Dimension dimensionFormworkRebarSide = doc.Create.NewDimension(view, dimensionLineRightSecond,
                                                                              refArrayRebarSide, ViewModel.SelectedDimensionType);
        } catch(Exception) { }
    }


    private void TryCreateTransverseViewDimensions(View view, bool onTopOfRebar) {
        var doc = Repository.Document;
        string rebarPart = onTopOfRebar ? "верх" : "низ";
        var dimensionBaseService = new DimensionBaseService(view, ViewModel.ParamValService);

        try {
            var rebarFinder = ViewModel.RebarFinder;
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }

            var grids = new FilteredElementCollector(doc, view.Id)
                .OfCategory(BuiltInCategory.OST_Grids)
                .Cast<Grid>()
                .ToList();


            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            // Размер по ФРОНТУ опалубка (положение снизу 1)
            Line dimensionLineBottomFirst = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Bottom, 1);
            ReferenceArray refArrayFormworkFront = dimensionBaseService.GetDimensionRefs(SheetInfo.HostElems[0] as FamilyInstance, '#', '/',
                                                                    new List<string>() { "фронт", "край" });
            Dimension dimensionFormworkFront = doc.Create.NewDimension(view, dimensionLineBottomFirst,
                                                                   refArrayFormworkFront, ViewModel.SelectedDimensionType);

            if(grids.Count > 0) {
                // Размер по ФРОНТУ опалубка + оси (положение сверху 1)
                Line dimensionLineTopSecond = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Top, 0.5);
                ReferenceArray refArrayFormworkGridFront = dimensionBaseService.GetDimensionRefs(grids, view, 
                                                                                                 new XYZ(0, 1, 0),
                                                                                                 refArrayFormworkFront);
                Dimension dimensionFormworkGridFront = doc.Create.NewDimension(view, dimensionLineTopSecond,
                                                                           refArrayFormworkGridFront, ViewModel.SelectedDimensionType);
            }

            if(!(onTopOfRebar && SheetInfo.RebarInfo.AllRebarAreL)) {
                // Размер по ФРОНТУ опалубка + армирование (положение снизу 2)
                Line dimensionLineBottomSecond = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Bottom, 0.5);
                // Добавляем ссылки на арматурные стержни
                ReferenceArray refArrayFormworkRebarFrontSecond = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                             new List<string>() { rebarPart, "фронт" },
                                                                         refArrayFormworkFront);
                Dimension dimensionFormworkRebarFrontFirst = doc.Create.NewDimension(view, dimensionLineBottomSecond,
                                                                                  refArrayFormworkRebarFrontSecond, ViewModel.SelectedDimensionType);
            }


            // Размер по ФРОНТУ опалубка + армирование в случае, если есть Г-стержни (положение снизу 0)
            if(onTopOfRebar && SheetInfo.RebarInfo.HasLRebar) {
                Line dimensionLineTopSecond = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Bottom, 0);
                // Добавляем ссылки на арматурные стержни
                ReferenceArray refArrayFormworkRebarFrontSecond = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                             new List<string>() { "низ", "фронт" },
                                                                         refArrayFormworkFront);
                Dimension dimensionFormworkRebarFrontSecond = doc.Create.NewDimension(view, dimensionLineTopSecond,
                                                                                  refArrayFormworkRebarFrontSecond, ViewModel.SelectedDimensionType);
            }


            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            // Размер по ТОРЦУ опалубка (положение справа 1)
            Line dimensionLineRightFirst = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Right, 1);
            ReferenceArray refArrayFormworkSide = dimensionBaseService.GetDimensionRefs(SheetInfo.HostElems[0] as FamilyInstance, '#', '/',
                                                                   new List<string>() { "торец", "край" });
            Dimension dimensionFormworkSide = doc.Create.NewDimension(view, dimensionLineRightFirst,
                                                                  refArrayFormworkSide, ViewModel.SelectedDimensionType);


            // Размер по ТОРЦУ опалубка + армирование (положение справа 2)
            Line dimensionLineRightSecond = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Right, 0.5);
            // Добавляем ссылки на арматурные стержни
            ReferenceArray refArrayFormworkRebarSide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                        new List<string>() { rebarPart, "торец" },
                                                                    refArrayFormworkSide);
            Dimension dimensionFormworkRebarSide = doc.Create.NewDimension(view, dimensionLineRightSecond,
                                                                              refArrayFormworkRebarSide, ViewModel.SelectedDimensionType);

            if(grids.Count > 0) {
                // Размер по ТОРЦУ опалубка + оси (положение слева 1)
                Line dimensionLineLeft = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Left, 1.2);
                ReferenceArray refArrayFormworkGridSide = dimensionBaseService.GetDimensionRefs(grids, view, 
                                                                                                new XYZ(1, 0, 0),
                                                                                                refArrayFormworkSide);
                Dimension dimensionFormworkGridSide = doc.Create.NewDimension(view, dimensionLineLeft,
                                                                          refArrayFormworkGridSide, ViewModel.SelectedDimensionType);

                // Корректируем концы осей, приближая их на виде к опалубке пилона, чтобы сократить габариты
                // видового экрана
                var transverseViewGridOffsets = new OffsetOption() {
                    LeftOffset = 1.5,
                    RightOffset = 0.5,
                    TopOffset = 0.6,
                    BottomOffset = 1.1
                };
                EditGridEnds(view, SheetInfo.HostElems.First() as FamilyInstance, grids, transverseViewGridOffsets, dimensionBaseService);
            }
        } catch(Exception) { }
    }


    private void EditGridEnds(View view, FamilyInstance familyInstance, 
                              List<Grid> grids, OffsetOption offsetOption, DimensionBaseService dimensionBaseService) {
        var rightDirection = view.RightDirection;

        foreach(var grid in grids) {
            var gridLine = grid.Curve as Line;
            var gridDir = gridLine.Direction;

            if(rightDirection.IsAlmostEqualTo(gridDir)
                || rightDirection.IsAlmostEqualTo(gridDir.Negate())) {

                var curve = grid.GetCurvesInView(DatumExtentType.ViewSpecific, view).First();

                Line offsetLine1 = dimensionBaseService.GetDimensionLine(familyInstance,
                                                                         DimensionOffsetType.Left,
                                                                         offsetOption.LeftOffset, false);
                var pt1 = curve.Project(offsetLine1.Origin).XYZPoint;

                Line offsetLine2 = dimensionBaseService.GetDimensionLine(familyInstance,
                                                                         DimensionOffsetType.Right,
                                                                         offsetOption.RightOffset, false);
                var pt2 = curve.Project(offsetLine2.Origin).XYZPoint;

                var newLine = Line.CreateBound(pt1, pt2);
                grid.SetCurveInView(DatumExtentType.ViewSpecific, view, newLine);

            } else {
                var curve = grid.GetCurvesInView(DatumExtentType.ViewSpecific, view).First();

                Line offsetLine1 = dimensionBaseService.GetDimensionLine(familyInstance,
                                                                         DimensionOffsetType.Bottom,
                                                                         offsetOption.BottomOffset, false);
                var pt1 = curve.Project(offsetLine1.Origin).XYZPoint;

                Line offsetLine2 = dimensionBaseService.GetDimensionLine(familyInstance,
                                                                         DimensionOffsetType.Top,
                                                                         offsetOption.TopOffset, false);
                var pt2 = curve.Project(offsetLine2.Origin).XYZPoint;

                var newLine = Line.CreateBound(pt1, pt2);
                grid.SetCurveInView(DatumExtentType.ViewSpecific, view, newLine);
            }
        }
    }
}
