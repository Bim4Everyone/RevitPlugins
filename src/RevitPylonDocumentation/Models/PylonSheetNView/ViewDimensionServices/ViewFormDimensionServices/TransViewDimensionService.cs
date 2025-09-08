using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.Services;
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


    internal void TryCreateDimensions(View view, bool refsForTop, bool pylonFromTop) {
        var doc = Repository.Document;
        string rebarPart = refsForTop ? "верх" : "низ";
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
            var pylon = pylonFromTop ?  SheetInfo.HostElems.Last() : SheetInfo.HostElems.First();
            var dimensionLineHostRef = refsForTop ? skeletonParentRebar : pylon;

            var refArrayFormworkFront = dimensionBaseService.GetDimensionRefs(pylon as FamilyInstance,
                                                                              '#', '/', ["фронт", "край"]);
            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            // Указывает нужно ли будет делать длинные оси снизу (сделано здесь, чтобы не выполнять запрос дважды)
            var longGridsWillBeNeeded = false;
            if(refsForTop) {
                // Когда все Гэшки
                if(SheetInfo.RebarInfo.AllRebarAreL) {
                    CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                    ["низ", "фронт", "край"], view, dimensionBaseService, refArrayFormworkFront, false);
                    longGridsWillBeNeeded = true;
                } else if(SheetInfo.RebarInfo.HasLRebar) {
                    // Когда Гэшки с одной стороны
                    if(rebarFinder.DirectionHasLRebar(view, SheetInfo.ProjectSection, DirectionType.Top)) {
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Top, 0.5,
                                        ["низ", "фронт", "край"], view, dimensionBaseService, refArrayFormworkFront, false);
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                        ["верх", "фронт", "край"], view, dimensionBaseService, refArrayFormworkFront, false);
                    } else {
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Top, 0.5,
                                        ["верх", "фронт", "край"], view, dimensionBaseService, refArrayFormworkFront, false);
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                        ["низ", "фронт", "край"], view, dimensionBaseService, refArrayFormworkFront, false);
                        longGridsWillBeNeeded = true;
                    }
                }
                // Когда Гэшек нет вообще
                if(!SheetInfo.RebarInfo.HasLRebar) {
                    CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                    ["верх", "фронт", "край"], view, dimensionBaseService, refArrayFormworkFront, false);
                }
            } else {
                CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                ["низ", "фронт", "край"], view, dimensionBaseService, refArrayFormworkFront, false);
            }
            // Определим отступ для размерной линии общего размера по опалубке (если есть верт оси, то будет дальше)
            var formworkFrontDimensionLineOffset = 1.0;
            // Размеры по осям
            if(grids.Count > 0 && dimensionBaseService.GetDimensionRefs(grids, view, XYZ.BasisY).Size > 0) {
                // Размер по ФРОНТУ опалубка + оси (положение сверху 1)
                CreateDimension(grids, dimensionLineHostRef, DirectionType.Bottom, 1, 
                                XYZ.BasisY, view, dimensionBaseService, refArrayFormworkFront);

                formworkFrontDimensionLineOffset = 1.5;
            }

            // Размер по ФРОНТУ опалубка (положение снизу 1.5)
            CreateDimension(refArrayFormworkFront, dimensionLineHostRef, DirectionType.Bottom,
                            formworkFrontDimensionLineOffset, view, dimensionBaseService);

            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            // Получаем референсы для размеров по крайним плоскостям пилона
            var refArrayFormworkSide = dimensionBaseService.GetDimensionRefs(pylon as FamilyInstance,
                                                                             '#', '/', ["торец", "край"]);
            // Размер по ТОРЦУ опалубка + армирование (положение справа ближнее)
            ReferenceArray refArraySide = default;
            // Если первый ряд - Гэшки, то берем по нижней плоскости
            if(SheetInfo.RebarInfo.FirstLRebarParamValue) {
                refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                     ["низ", "торец", "_1_"], 
                                                                     oldRefArray: refArrayFormworkSide);
            } else {
                refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                     [rebarPart, "торец", "_1_"],
                                                                     oldRefArray: refArrayFormworkSide);
            }
            // Если второй ряд - Гэшки, то берем по нижней плоскости
            if(SheetInfo.RebarInfo.SecondLRebarParamValue) {
                refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                     ["низ", "торец", "_2_"],
                                                                     oldRefArray: refArraySide);
            } else {
                refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                     [rebarPart, "торец", "_2_"],
                                                                     oldRefArray: refArraySide);
            }
            CreateDimension(refArraySide, pylon, DirectionType.Right, 0.5, view, dimensionBaseService, false);

            // Размер по ТОРЦУ опалубка (положение справа дальнее)
            CreateDimension(refArrayFormworkSide, pylon, DirectionType.Right, 1, view, dimensionBaseService);

            // Если на виде есть оси, то создаем размер
            if(grids.Count > 0) {
                // Размер по ТОРЦУ опалубка + оси (положение слева дальнее)
                CreateDimension(grids, pylon, DirectionType.Left, 1, XYZ.BasisX, view,
                                dimensionBaseService, refArrayFormworkSide);
            }

            if(grids.Count > 0) {
                // Корректируем концы осей, приближая их на виде к опалубке пилона, чтобы сократить габариты
                // видового экрана
                var transverseViewGridOffsets = new OffsetOption() {
                    LeftOffset = 1.5,
                    RightOffset = 0.3,
                    TopOffset = 0.2,
                    BottomOffset = 1.6
                };
                
                // В случае, если на виде вниз будут смотреть Гэшки, то нужно оставить больше места
                if(longGridsWillBeNeeded) {
                    transverseViewGridOffsets.BottomOffset = 3.0;
                }
                EditGridEnds(view, pylon, grids, transverseViewGridOffsets,
                             dimensionBaseService);
            }
        } catch(Exception) { }
    }


    private void CreateDimension(Element dimensioningElement, Element elemForOffset,
                                 DirectionType directionType, double offsetCoefficient,
                                 List<string> importantRefNameParts, View view,
                                 DimensionBaseService dimensionBaseService, ReferenceArray oldRefArray = null, 
                                 bool needEqualityFormula = true) {

        if(dimensioningElement is null || (importantRefNameParts?.Count ?? 0) == 0) { return; }
        var dimensionLine = dimensionBaseService.GetDimensionLine(elemForOffset, directionType, offsetCoefficient);
        ReferenceArray refArray = dimensionBaseService.GetDimensionRefs(dimensioningElement as FamilyInstance, '#', '/',
                                                                        importantRefNameParts, oldRefArray: oldRefArray);
        var dimension = Repository.Document.Create.NewDimension(view, dimensionLine, refArray, 
                                                                ViewModel.SelectedDimensionType);
        if(needEqualityFormula) {
            dimension.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, 2);
        }
    }


    private void CreateDimension(ReferenceArray oldRefArray, Element elemForOffset,
                                 DirectionType directionType, double offsetCoefficient, View view,
                                 DimensionBaseService dimensionBaseService, bool needEqualityFormula = true) {
        
        var dimensionLine = dimensionBaseService.GetDimensionLine(elemForOffset, directionType, offsetCoefficient);
        var dimension = Repository.Document.Create.NewDimension(view, dimensionLine, oldRefArray,
                                                                ViewModel.SelectedDimensionType);
        if(needEqualityFormula) {
            dimension.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, 2);
        }
    }


    private void CreateDimension(List<Grid> grids, Element elemForOffset, DirectionType directionType, 
                                 double offsetCoefficient, XYZ gridsDirection, View view,
                                 DimensionBaseService dimensionBaseService, ReferenceArray oldRefArray = null) {
        if((grids?.Count ?? 0) == 0) { return; }
        var dimensionLineLeft = dimensionBaseService.GetDimensionLine(elemForOffset, directionType, 
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

                var offsetLine1 = dimensionBaseService.GetDimensionLine(rebar, DirectionType.Left,
                                                                        offsetOption.LeftOffset);
                var pt1 = curve.Project(offsetLine1.Origin).XYZPoint;

                var offsetLine2 = dimensionBaseService.GetDimensionLine(rebar, DirectionType.Right,
                                                                        offsetOption.RightOffset);
                var pt2 = curve.Project(offsetLine2.Origin).XYZPoint;

                var newLine = Line.CreateBound(pt1, pt2);
                grid.SetCurveInView(DatumExtentType.ViewSpecific, view, newLine);

            } else {
                var curve = grid.GetCurvesInView(DatumExtentType.ViewSpecific, view).First();

                var offsetLine1 = dimensionBaseService.GetDimensionLine(rebar, DirectionType.Bottom,
                                                                        offsetOption.BottomOffset);
                var pt1 = curve.Project(offsetLine1.Origin).XYZPoint;

                var offsetLine2 = dimensionBaseService.GetDimensionLine(rebar, DirectionType.Top,
                                                                        offsetOption.TopOffset);
                var pt2 = curve.Project(offsetLine2.Origin).XYZPoint;

                var newLine = Line.CreateBound(pt1, pt2);
                grid.SetCurveInView(DatumExtentType.ViewSpecific, view, newLine);
            }
        }
    }
}
