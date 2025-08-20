using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitPylonDocumentation.ViewModels;

using Grid = Autodesk.Revit.DB.Grid;

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



    /// <summary>
    /// Определение с какой стороны относительно вид находится Г-образный стержень
    /// </summary>
    internal bool LRebarIsUp(View view) {
        var rebarFinder = ViewModel.RebarFinder;
        // Г-образный стержень
        var lRebar = rebarFinder.GetSimpleRebars(view, SheetInfo.ProjectSection, 1101).FirstOrDefault();
        // Бутылка
        var bottleRebar = rebarFinder.GetSimpleRebars(view, SheetInfo.ProjectSection, 1204).FirstOrDefault();

        if(lRebar is null || bottleRebar is null) {
            return false;
        }

        var lRebarLocation = lRebar.Location as LocationPoint;
        var lRebarPt = lRebarLocation.Point;

        var bottleRebarLocation = bottleRebar.Location as LocationPoint;
        var bottleRebarPt = bottleRebarLocation.Point;

        var transform = view.CropBox.Transform;
        var inverseTransform = transform.Inverse;
        // Получаем координаты точек вставки в координатах вида
        var lRebarPtTransformed = inverseTransform.OfPoint(lRebarPt);
        var bottleRebarPtTransformed = inverseTransform.OfPoint(bottleRebarPt);

        return lRebarPtTransformed.Y > bottleRebarPtTransformed.Y;
    }



    private void CreateTopBottomDimension(Element dimensioningElement, Element hostRefForOffset, 
                                          DimensionOffsetType dimensionOffsetType, double offsetCoefficient, 
                                          string rebarPart, View view, DimensionBaseService dimensionBaseService, 
                                          ReferenceArray oldRefArray = null) {
        var dimensionLineTopSecond = dimensionBaseService.GetDimensionLine(hostRefForOffset,
                                                                           dimensionOffsetType, offsetCoefficient);
        // Добавляем ссылки на арматурные стержни
        var refArrayFormworkRebarFrontSecond =
            dimensionBaseService.GetDimensionRefs(dimensioningElement as FamilyInstance, '#', '/', [rebarPart, "фронт"],
                                                  oldRefArray: oldRefArray);
        var dimension = Repository.Document.Create.NewDimension(view, dimensionLineTopSecond, refArrayFormworkRebarFrontSecond,
                                ViewModel.SelectedDimensionType);
        dimension.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, 2);
    }


    internal void TryCreateDimensions(View view, bool onTopOfRebar) {
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
            var pylon = onTopOfRebar ?  SheetInfo.HostElems.Last() : SheetInfo.HostElems.First();
            var dimensionLineHostRef = onTopOfRebar ? skeletonParentRebar : pylon;


            var refArrayFormworkFront = dimensionBaseService.GetDimensionRefs(pylon as FamilyInstance,
                                                                              '#', '/', ["фронт", "край"]);
            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            if(onTopOfRebar) {
                // Когда все Гэшки
                if(SheetInfo.RebarInfo.AllRebarAreL) {
                    CreateTopBottomDimension(skeletonParentRebar, dimensionLineHostRef, DimensionOffsetType.Bottom, 0.5,
                                             "низ", view, dimensionBaseService, refArrayFormworkFront);
                } else if(SheetInfo.RebarInfo.HasLRebar) {
                    // Когда Гэшки с одной стороны
                    if(LRebarIsUp(view)) {
                        CreateTopBottomDimension(skeletonParentRebar, dimensionLineHostRef, DimensionOffsetType.Top, 0.5,
                                                 "низ", view, dimensionBaseService, refArrayFormworkFront);
                        CreateTopBottomDimension(skeletonParentRebar, dimensionLineHostRef, DimensionOffsetType.Bottom, 0.5,
                                                 "верх", view, dimensionBaseService, refArrayFormworkFront);
                    } else {
                        CreateTopBottomDimension(skeletonParentRebar, dimensionLineHostRef, DimensionOffsetType.Top, 0.5,
                                                 "верх", view, dimensionBaseService, refArrayFormworkFront);
                        CreateTopBottomDimension(skeletonParentRebar, dimensionLineHostRef, DimensionOffsetType.Bottom, 0.5,
                                                 "низ", view, dimensionBaseService, refArrayFormworkFront);
                    }
                }
                // Когда Гэшек нет вообще
                if(!SheetInfo.RebarInfo.HasLRebar) {
                    CreateTopBottomDimension(skeletonParentRebar, dimensionLineHostRef, DimensionOffsetType.Bottom, 0.5,
                                             "верх", view, dimensionBaseService, refArrayFormworkFront);
                }
            } else {
                CreateTopBottomDimension(skeletonParentRebar, dimensionLineHostRef, DimensionOffsetType.Bottom, 0.5,
                                         "низ", view, dimensionBaseService, refArrayFormworkFront);
            }

            // Размер по ФРОНТУ опалубка (положение снизу 1)
            var dimensionLineBottomFirst = dimensionBaseService.GetDimensionLine(dimensionLineHostRef,
                                                                                 DimensionOffsetType.Bottom, 1);
            var dimensionFormworkFront = doc.Create.NewDimension(view, dimensionLineBottomFirst,
                                                                 refArrayFormworkFront, ViewModel.SelectedDimensionType);

            if(grids.Count > 0) {
                double gridDimensionLineOffset =
                    onTopOfRebar && !SheetInfo.RebarInfo.AllRebarAreL && SheetInfo.RebarInfo.HasLRebar ? 1 : 0.5;

                // Размер по ФРОНТУ опалубка + оси (положение сверху 1)
                var dimensionLineTopSecond = dimensionBaseService.GetDimensionLine(dimensionLineHostRef,
                                                                                   DimensionOffsetType.Top,
                                                                                   gridDimensionLineOffset);
                var refArrayFormworkGridFront = dimensionBaseService.GetDimensionRefs(grids, view, XYZ.BasisY,
                                                                                      refArrayFormworkFront);
                var dimensionFormworkGridFront = doc.Create.NewDimension(view, dimensionLineTopSecond,
                                                                         refArrayFormworkGridFront,
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
            var dimensionLineRightSecond = dimensionBaseService.GetDimensionLine(pylon, DimensionOffsetType.Right, 0.5);
            // Добавляем ссылки на арматурные стержни
            var refArrayFormworkRebarSide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                                  [rebarPart, "торец"],
                                                                                  oldRefArray: refArrayFormworkSide);
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
                EditGridEnds(view, pylon, grids, transverseViewGridOffsets, 
                             dimensionBaseService);
            }
        } catch(Exception) { }
    }


    private void EditGridEnds(View view, Element rebar, List<Grid> grids, OffsetOption offsetOption, 
                              DimensionBaseService dimensionBaseService) {
        if(view is null || rebar is null) { return; }
        var rightDirection = view.RightDirection;

        foreach(var grid in grids) {
            var gridLine = grid.Curve as Line;
            var gridDir = gridLine.Direction;

            if(rightDirection.IsAlmostEqualTo(gridDir)
                || rightDirection.IsAlmostEqualTo(gridDir.Negate())) {

                var curve = grid.GetCurvesInView(DatumExtentType.ViewSpecific, view).First();

                var offsetLine1 = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Left,
                                                                        offsetOption.LeftOffset);
                var pt1 = curve.Project(offsetLine1.Origin).XYZPoint;

                var offsetLine2 = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Right,
                                                                        offsetOption.RightOffset);
                var pt2 = curve.Project(offsetLine2.Origin).XYZPoint;

                var newLine = Line.CreateBound(pt1, pt2);
                grid.SetCurveInView(DatumExtentType.ViewSpecific, view, newLine);

            } else {
                var curve = grid.GetCurvesInView(DatumExtentType.ViewSpecific, view).First();

                var offsetLine1 = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Bottom,
                                                                        offsetOption.BottomOffset);
                var pt1 = curve.Project(offsetLine1.Origin).XYZPoint;

                var offsetLine2 = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Top,
                                                                        offsetOption.TopOffset);
                var pt2 = curve.Project(offsetLine2.Origin).XYZPoint;

                var newLine = Line.CreateBound(pt1, pt2);
                grid.SetCurveInView(DatumExtentType.ViewSpecific, view, newLine);
            }
        }
    }
}
