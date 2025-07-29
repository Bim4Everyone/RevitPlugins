using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewFormDimensionServices;
internal class TransViewDimensionService {

    internal TransViewDimensionService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }

    internal void TryCreateTransverseViewDimensions(View view, bool onTopOfRebar) {
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

            // Определяем относительно чего нужно строить размерные линии - каркаса или пилона
            var pylon = SheetInfo.HostElems.First();
            var dimensionLineHostRef = onTopOfRebar ? skeletonParentRebar : pylon;

            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            // Размер по ФРОНТУ опалубка (положение снизу 1)
            var dimensionLineBottomFirst = dimensionBaseService.GetDimensionLine(dimensionLineHostRef, 
                                                                                 DimensionOffsetType.Bottom, 1);
            var refArrayFormworkFront = dimensionBaseService.GetDimensionRefs(pylon as FamilyInstance, 
                                                                              '#', '/', ["фронт", "край"]);
            var dimensionFormworkFront = doc.Create.NewDimension(view, dimensionLineBottomFirst,
                                                                 refArrayFormworkFront, ViewModel.SelectedDimensionType);
            if(grids.Count > 0) {
                // Размер по ФРОНТУ опалубка + оси (положение сверху 1)
                var dimensionLineTopSecond = dimensionBaseService.GetDimensionLine(dimensionLineHostRef, 
                                                                                   DimensionOffsetType.Top, 0.5);
                var refArrayFormworkGridFront = dimensionBaseService.GetDimensionRefs(grids, view, XYZ.BasisY,
                                                                                      refArrayFormworkFront);
                var dimensionFormworkGridFront = doc.Create.NewDimension(view, dimensionLineTopSecond,
                                                                         refArrayFormworkGridFront, 
                                                                         ViewModel.SelectedDimensionType);
            }
            if(!(onTopOfRebar && SheetInfo.RebarInfo.AllRebarAreL)) {
                // Размер по ФРОНТУ опалубка + армирование (положение снизу 2)
                var dimensionLineBottomSecond = dimensionBaseService.GetDimensionLine(dimensionLineHostRef, 
                                                                                      DimensionOffsetType.Bottom, 0.5);
                // Добавляем ссылки на арматурные стержни
                var refArrayFormworkRebarFrontSecond = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, 
                                                                                             '#', '/',
                                                                                             [rebarPart, "фронт"],
                                                                                             refArrayFormworkFront);
                doc.Create.NewDimension(view, dimensionLineBottomSecond, refArrayFormworkRebarFrontSecond, 
                                        ViewModel.SelectedDimensionType);
            }
            // Размер по ФРОНТУ опалубка + армирование в случае, если есть Г-стержни (положение снизу 0)
            if(onTopOfRebar && SheetInfo.RebarInfo.HasLRebar) {
                var dimensionLineTopSecond = dimensionBaseService.GetDimensionLine(dimensionLineHostRef, 
                                                                                   DimensionOffsetType.Bottom, 0);
                // Добавляем ссылки на арматурные стержни
                var refArrayFormworkRebarFrontSecond = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, 
                                                                                             '#', '/',
                                                                                             ["низ", "фронт"],
                                                                                             refArrayFormworkFront);
                doc.Create.NewDimension(view, dimensionLineTopSecond, refArrayFormworkRebarFrontSecond, 
                                        ViewModel.SelectedDimensionType);
            }

            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            // Размер по ТОРЦУ опалубка (положение справа 1)
            var dimensionLineRightFirst = dimensionBaseService.GetDimensionLine(pylon, DimensionOffsetType.Right, 0.8);
            var refArrayFormworkSide = dimensionBaseService.GetDimensionRefs(pylon as FamilyInstance, 
                                                                             '#', '/', ["торец", "край"]);
            var dimensionFormworkSide = doc.Create.NewDimension(view, dimensionLineRightFirst,
                                                                  refArrayFormworkSide, ViewModel.SelectedDimensionType);

            // Размер по ТОРЦУ опалубка + армирование (положение справа 2)
            var dimensionLineRightSecond = dimensionBaseService.GetDimensionLine(pylon, DimensionOffsetType.Right, 0.3);
            // Добавляем ссылки на арматурные стержни
            var refArrayFormworkRebarSide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                                  [rebarPart, "торец"],
                                                                                  refArrayFormworkSide);
            var dimensionFormworkRebarSide = doc.Create.NewDimension(view, dimensionLineRightSecond,
                                                                     refArrayFormworkRebarSide, 
                                                                     ViewModel.SelectedDimensionType);
            if(grids.Count > 0) {
                // Размер по ТОРЦУ опалубка + оси (положение слева 1)
                var dimensionLineLeft = dimensionBaseService.GetDimensionLine(pylon, DimensionOffsetType.Left, 1);
                var refArrayFormworkGridSide = dimensionBaseService.GetDimensionRefs(grids, view, XYZ.BasisX,
                                                                                     refArrayFormworkSide);
                var dimensionFormworkGridSide = doc.Create.NewDimension(view, dimensionLineLeft,
                                                                        refArrayFormworkGridSide, 
                                                                        ViewModel.SelectedDimensionType);
                // Корректируем концы осей, приближая их на виде к опалубке пилона, чтобы сократить габариты
                // видового экрана
                var transverseViewGridOffsets = new OffsetOption() {
                    LeftOffset = 1.5,
                    RightOffset = 0.5,
                    TopOffset = 0.6,
                    BottomOffset = 1.1
                };
                EditGridEnds(view, pylon as FamilyInstance, grids, transverseViewGridOffsets, 
                             dimensionBaseService);
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

                var offsetLine1 = dimensionBaseService.GetDimensionLine(familyInstance,
                                                                        DimensionOffsetType.Left,
                                                                        offsetOption.LeftOffset, false);
                var pt1 = curve.Project(offsetLine1.Origin).XYZPoint;

                var offsetLine2 = dimensionBaseService.GetDimensionLine(familyInstance,
                                                                        DimensionOffsetType.Right,
                                                                        offsetOption.RightOffset, false);
                var pt2 = curve.Project(offsetLine2.Origin).XYZPoint;

                var newLine = Line.CreateBound(pt1, pt2);
                grid.SetCurveInView(DatumExtentType.ViewSpecific, view, newLine);

            } else {
                var curve = grid.GetCurvesInView(DatumExtentType.ViewSpecific, view).First();

                var offsetLine1 = dimensionBaseService.GetDimensionLine(familyInstance,
                                                                        DimensionOffsetType.Bottom,
                                                                        offsetOption.BottomOffset, false);
                var pt1 = curve.Project(offsetLine1.Origin).XYZPoint;

                var offsetLine2 = dimensionBaseService.GetDimensionLine(familyInstance,
                                                                        DimensionOffsetType.Top,
                                                                        offsetOption.TopOffset, false);
                var pt2 = curve.Project(offsetLine2.Origin).XYZPoint;

                var newLine = Line.CreateBound(pt1, pt2);
                grid.SetCurveInView(DatumExtentType.ViewSpecific, view, newLine);
            }
        }
    }
}
