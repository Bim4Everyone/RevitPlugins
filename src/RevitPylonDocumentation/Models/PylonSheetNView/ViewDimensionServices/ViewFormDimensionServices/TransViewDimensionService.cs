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
    private readonly DimensionBaseService _dimensionBaseService;
    private readonly DimensionSegmentsService _dimSegmentsService;
    private readonly GridEndsService _gridEndsService;

    internal TransViewDimensionService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                       PylonView pylonView, DimensionBaseService dimensionBaseService) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        ViewOfPylon = pylonView;
        _dimensionBaseService = dimensionBaseService;

        _dimSegmentsService = new DimensionSegmentsService(pylonView.ViewElement);
        _gridEndsService = new GridEndsService(ViewOfPylon.ViewElement, dimensionBaseService);
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }
    internal PylonView ViewOfPylon { get; set; }


    internal void TryCreateDimensions(bool refsForTop, bool pylonFromTop) {
        var doc = Repository.Document;
        var view = ViewOfPylon.ViewElement;
        string rebarPart = refsForTop ? "верх" : "низ";
        
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

            var refArrayFormworkFront = _dimensionBaseService.GetDimensionRefs(pylon as FamilyInstance,
                                                                              '#', '/', ["фронт", "край"]);
            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            // Указывает нужно ли будет делать длинные оси снизу (сделано здесь, чтобы не выполнять запрос дважды)
            var vertDimensionsForEdit = new List<Dimension>();
            var longGridsWillBeNeeded = false;
            if(refsForTop) {
                // Когда все Гэшки
                if(SheetInfo.RebarInfo.AllRebarAreL) {
                    vertDimensionsForEdit.Add(
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                        ["низ", "фронт", "край"], view, 
                                        refArrayFormworkFront, false));
                    longGridsWillBeNeeded = true;
                } else if(SheetInfo.RebarInfo.HasLRebar) {
                    // Когда Гэшки с одной стороны
                    if(rebarFinder.DirectionHasLRebar(view, SheetInfo.ProjectSection, DirectionType.Top)) {
                        vertDimensionsForEdit.Add(
                            CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Top, 0.5,
                                            ["низ", "фронт", "край"], view, 
                                            refArrayFormworkFront, false));
                        vertDimensionsForEdit.Add(
                            CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                            ["верх", "фронт", "край"], view, 
                                            refArrayFormworkFront, false));
                    } else {
                        vertDimensionsForEdit.Add(
                            CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Top, 0.5,
                                            ["верх", "фронт", "край"], view, 
                                            refArrayFormworkFront, false));
                        vertDimensionsForEdit.Add(
                            CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                            ["низ", "фронт", "край"], view, 
                                            refArrayFormworkFront, false));
                        longGridsWillBeNeeded = true;
                    }
                }
                // Когда Гэшек нет вообще
                if(!SheetInfo.RebarInfo.HasLRebar) {
                    vertDimensionsForEdit.Add(
                        CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                        ["верх", "фронт", "край"], view, 
                                        refArrayFormworkFront, false));
                }
            } else {
                vertDimensionsForEdit.Add(
                    CreateDimension(skeletonParentRebar, dimensionLineHostRef, DirectionType.Bottom, 0.5,
                                    ["низ", "фронт", "край"], view, refArrayFormworkFront, false));
            }

            // Изменяем размер, передвигая текст у крайних сегментов для корректного отображения
            foreach(var dimension in vertDimensionsForEdit) {
                //EditThreeSegmentsDimension(dimension, true);

                _dimSegmentsService.EditEdgeDimensionSegments(dimension,
                                                              _dimSegmentsService.VertSmallUpDirectDimTextOffset,
                                                              _dimSegmentsService.VertSmallUpInvertedDimTextOffset);
            }

            // Определим отступ для размерной линии общего размера по опалубке (если есть верт оси, то будет дальше)
            var formworkFrontDimensionLineOffset = 1.0;
            // Размеры по осям
            if(grids.Count > 0 && _dimensionBaseService.GetDimensionRefs(grids, view, XYZ.BasisY).Size > 0) {
                // Размер по ФРОНТУ опалубка + оси (положение сверху 1)
                CreateDimension(grids, dimensionLineHostRef, DirectionType.Bottom, 1, 
                                XYZ.BasisY, view, refArrayFormworkFront);

                formworkFrontDimensionLineOffset = 1.5;
            }

            // Размер по ФРОНТУ опалубка (положение снизу 1.5)
            CreateDimension(refArrayFormworkFront, dimensionLineHostRef, DirectionType.Bottom,
                            formworkFrontDimensionLineOffset, view);

            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            // Получаем референсы для размеров по крайним плоскостям пилона
            var refArrayFormworkSide = _dimensionBaseService.GetDimensionRefs(pylon as FamilyInstance,
                                                                             '#', '/', ["торец", "край"]);
            // Размер по ТОРЦУ опалубка + армирование (положение справа ближнее)
            ReferenceArray refArraySide = default;
            // Если первый ряд - Гэшки, то берем по нижней плоскости
            if(SheetInfo.RebarInfo.FirstLRebarParamValue) {
                refArraySide = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                     ["низ", "торец", "_1_"], 
                                                                     oldRefArray: refArrayFormworkSide);
            } else {
                refArraySide = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                     [rebarPart, "торец", "_1_"],
                                                                     oldRefArray: refArrayFormworkSide);
            }
            // Если второй ряд - Гэшки, то берем по нижней плоскости
            if(SheetInfo.RebarInfo.SecondLRebarParamValue) {
                refArraySide = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                     ["низ", "торец", "_2_"],
                                                                     oldRefArray: refArraySide);
            } else {
                refArraySide = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                     [rebarPart, "торец", "_2_"],
                                                                     oldRefArray: refArraySide);
            }
            var formworkRebarDimensionSide = CreateDimension(refArraySide, pylon, DirectionType.Right, 0.6, 
                                                             view, false);
            // Изменяем размер, передвигая текст у крайних сегментов для корректного отображения
            _dimSegmentsService.EditEdgeDimensionSegments(formworkRebarDimensionSide,
                                                          _dimSegmentsService.HorizSmallUpDirectDimTextOffset,
                                                          _dimSegmentsService.HorizSmallUpInvertedDimTextOffset);

            // Размер по ТОРЦУ опалубка (положение справа дальнее)
            CreateDimension(refArrayFormworkSide, pylon, DirectionType.Right, 1, view);

            // Если на виде есть оси, то создаем размер
            if(grids.Count > 0) {
                // Размер по ТОРЦУ опалубка + оси (положение слева дальнее)
                CreateDimension(grids, pylon, DirectionType.Left, 1, XYZ.BasisX, view, refArrayFormworkSide);
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
                _gridEndsService.EditGridEnds(pylon, grids, transverseViewGridOffsets);
            }
        } catch(Exception) { }
    }

    private Dimension CreateDimension(Element dimensioningElement, Element elemForOffset,
                                 DirectionType directionType, double offsetCoefficient,
                                 List<string> importantRefNameParts, View view, ReferenceArray oldRefArray = null, 
                                 bool needEqualityFormula = true) {

        if(dimensioningElement is null || (importantRefNameParts?.Count ?? 0) == 0) { return null; }
        var dimensionLine = _dimensionBaseService.GetDimensionLine(elemForOffset, directionType, offsetCoefficient);
        ReferenceArray refArray = _dimensionBaseService.GetDimensionRefs(dimensioningElement as FamilyInstance, '#', '/',
                                                                        importantRefNameParts, oldRefArray: oldRefArray);
        var dimension = Repository.Document.Create.NewDimension(view, dimensionLine, refArray, 
                                                                ViewModel.SelectedDimensionType);
        if(needEqualityFormula) {
            dimension.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, 2);
        }
        return dimension;
    }


    private Dimension CreateDimension(ReferenceArray oldRefArray, Element elemForOffset,
                                 DirectionType directionType, double offsetCoefficient, View view,
                                 bool needEqualityFormula = true) {
        
        var dimensionLine = _dimensionBaseService.GetDimensionLine(elemForOffset, directionType, offsetCoefficient);
        var dimension = Repository.Document.Create.NewDimension(view, dimensionLine, oldRefArray,
                                                                ViewModel.SelectedDimensionType);
        if(needEqualityFormula) {
            dimension.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, 2);
        }
        return dimension;
    }


    private void CreateDimension(List<Grid> grids, Element elemForOffset, DirectionType directionType, 
                                 double offsetCoefficient, XYZ gridsDirection, View view,
                                 ReferenceArray oldRefArray = null) {
        if((grids?.Count ?? 0) == 0) { return; }
        var dimensionLineLeft = _dimensionBaseService.GetDimensionLine(elemForOffset, directionType, 
                                                                      offsetCoefficient);
        var refArrayFormworkGridSide = _dimensionBaseService.GetDimensionRefs(grids, view, gridsDirection,
                                                                             oldRefArray);
        if(refArrayFormworkGridSide.Size != oldRefArray.Size) {
            Repository.Document.Create.NewDimension(view, dimensionLineLeft, refArrayFormworkGridSide,
                                                    ViewModel.SelectedDimensionType);
        }
    }
}
