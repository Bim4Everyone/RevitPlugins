using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

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
            // Указывает нужно ли будет делать длинные оси снизу (сделано здесь, чтобы не выполнять запрос дважды)
            var longGridsWillBeNeeded = false;
            if(onTopOfRebar) {
                // Когда все Гэшки
                if(SheetInfo.RebarInfo.AllRebarAreL) {
                    CreateDimension(skeletonParentRebar, dimensionLineHostRef, DimensionOffsetType.Bottom, 0.5,
                                    ["низ", "фронт", "край"], view, dimensionBaseService, refArrayFormworkFront, false);
                    longGridsWillBeNeeded = true;
                } else if(SheetInfo.RebarInfo.HasLRebar) {
                    // Когда Гэшки с одной стороны
                    if(rebarFinder.DirectionHasLRebar(view, SheetInfo.ProjectSection, DirectionType.Top)) {
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DimensionOffsetType.Top, 0.5,
                                        ["низ", "фронт", "край"], view, dimensionBaseService, refArrayFormworkFront, false);
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DimensionOffsetType.Bottom, 0.5,
                                        ["верх", "фронт", "край"], view, dimensionBaseService, refArrayFormworkFront, false);
                    } else {
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DimensionOffsetType.Top, 0.5,
                                        ["верх", "фронт", "край"], view, dimensionBaseService, refArrayFormworkFront, false);
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DimensionOffsetType.Bottom, 0.5,
                                        ["низ", "фронт", "край"], view, dimensionBaseService, refArrayFormworkFront, false);
                        longGridsWillBeNeeded = true;
                    }
                }
                // Когда Гэшек нет вообще
                if(!SheetInfo.RebarInfo.HasLRebar) {
                    CreateDimension(skeletonParentRebar, dimensionLineHostRef, DimensionOffsetType.Bottom, 0.5,
                                    ["верх", "фронт", "край"], view, dimensionBaseService, refArrayFormworkFront, false);
                }
            } else {
                CreateDimension(skeletonParentRebar, dimensionLineHostRef, DimensionOffsetType.Bottom, 0.5,
                                ["низ", "фронт", "край"], view, dimensionBaseService, refArrayFormworkFront, false);
            }
            // Размер по ФРОНТУ опалубка (положение снизу 1)
            CreateDimension(refArrayFormworkFront, dimensionLineHostRef, DimensionOffsetType.Bottom, 1, view, 
                            dimensionBaseService);

            if(grids.Count > 0) {
                double gridDimensionLineOffset =
                    onTopOfRebar && !SheetInfo.RebarInfo.AllRebarAreL && SheetInfo.RebarInfo.HasLRebar ? 1 : 0.5;

                // Размер по ФРОНТУ опалубка + оси (положение сверху 1)
                CreateDimension(grids, dimensionLineHostRef, DimensionOffsetType.Top, gridDimensionLineOffset, 
                                XYZ.BasisY, view, dimensionBaseService, refArrayFormworkFront);
            }

            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            // Получаем референсы для размеров по крайним плоскостям пилона
            var refArrayFormworkSide = dimensionBaseService.GetDimensionRefs(pylon as FamilyInstance,
                                                                             '#', '/', ["торец", "край"]);
            // Размер по ТОРЦУ опалубка + армирование (положение справа ближнее)
            CreateDimension(skeletonParentRebar, pylon, DimensionOffsetType.Right, 0.5,
                            [rebarPart, "торец"], view, dimensionBaseService, refArrayFormworkSide, false);

            // Размер по ТОРЦУ опалубка (положение справа дальнее)
            CreateDimension(refArrayFormworkSide, pylon, DimensionOffsetType.Right, 1, view, dimensionBaseService);

            // Если на виде есть оси, то создаем размер
            if(grids.Count > 0) {
                // Размер по ТОРЦУ опалубка + оси (положение слева дальнее)
                CreateDimension(grids, pylon, DimensionOffsetType.Left, 1, XYZ.BasisX, view,
                                dimensionBaseService, refArrayFormworkSide);
            }

            if(grids.Count > 0) {
                // Корректируем концы осей, приближая их на виде к опалубке пилона, чтобы сократить габариты
                // видового экрана
                var transverseViewGridOffsets = new OffsetOption() {
                    LeftOffset = 1.5,
                    RightOffset = 0.5,
                    TopOffset = 0.6,
                    BottomOffset = 1.1
                };
                // В случае, если на виде вниз будут смотреть Гэшки, то нужно оставить больше места
                if(longGridsWillBeNeeded) {
                    transverseViewGridOffsets.BottomOffset = 2.6;
                }
                EditGridEnds(view, pylon, grids, transverseViewGridOffsets,
                             dimensionBaseService);
            }
        } catch(Exception) { }
    }


    private void CreateDimension(Element dimensioningElement, Element elemForOffset,
                                 DimensionOffsetType dimensionOffsetType, double offsetCoefficient,
                                 List<string> importantRefNameParts, View view,
                                 DimensionBaseService dimensionBaseService, ReferenceArray oldRefArray = null, 
                                 bool needEqualityFormula = true) {

        if(dimensioningElement is null || (importantRefNameParts?.Count ?? 0) == 0) { return; }
        var dimensionLine = dimensionBaseService.GetDimensionLine(elemForOffset, dimensionOffsetType, offsetCoefficient);
        ReferenceArray refArray = dimensionBaseService.GetDimensionRefs(dimensioningElement as FamilyInstance, '#', '/',
                                                                        importantRefNameParts, oldRefArray: oldRefArray);
        var dimension = Repository.Document.Create.NewDimension(view, dimensionLine, refArray, 
                                                                ViewModel.SelectedDimensionType);
        if(needEqualityFormula) {
            dimension.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, 2);
        }
    }


    private void CreateDimension(ReferenceArray oldRefArray, Element elemForOffset,
                                 DimensionOffsetType dimensionOffsetType, double offsetCoefficient, View view,
                                 DimensionBaseService dimensionBaseService, bool needEqualityFormula = true) {
        
        var dimensionLine = dimensionBaseService.GetDimensionLine(elemForOffset, dimensionOffsetType, offsetCoefficient);
        var dimension = Repository.Document.Create.NewDimension(view, dimensionLine, oldRefArray,
                                                                ViewModel.SelectedDimensionType);
        if(needEqualityFormula) {
            dimension.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, 2);
        }
    }


    private void CreateDimension(List<Grid> grids, Element elemForOffset, DimensionOffsetType dimensionOffsetType, 
                                 double offsetCoefficient, XYZ gridsDirection, View view,
                                 DimensionBaseService dimensionBaseService, ReferenceArray oldRefArray = null) {
        if((grids?.Count ?? 0) == 0) { return; }
        var dimensionLineLeft = dimensionBaseService.GetDimensionLine(elemForOffset, dimensionOffsetType, 
                                                                      offsetCoefficient);
        var refArrayFormworkGridSide = dimensionBaseService.GetDimensionRefs(grids, view, gridsDirection,
                                                                             oldRefArray);
        if(refArrayFormworkGridSide.Size != oldRefArray.Size) {
            Repository.Document.Create.NewDimension(view, dimensionLineLeft, refArrayFormworkGridSide,
                                                    ViewModel.SelectedDimensionType);
        }
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
