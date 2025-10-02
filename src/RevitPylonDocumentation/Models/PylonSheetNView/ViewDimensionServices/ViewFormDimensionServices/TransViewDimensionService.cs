using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

using Grid = Autodesk.Revit.DB.Grid;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewFormDimensionServices;
internal class TransViewDimensionService {
    private readonly MainViewModel _viewModel;
    private readonly RevitRepository _repository;
    private readonly PylonSheetInfo _sheetInfo;
    private readonly PylonView _viewOfPylon;

    private readonly DimensionBaseService _dimensionBaseService;
    private readonly DimensionSegmentsService _dimSegmentsService;
    private readonly GridEndsService _gridEndsService;
    private readonly DimensionCreationService _dimCreationService;

    internal TransViewDimensionService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                       PylonView pylonView, DimensionBaseService dimensionBaseService) {
        _viewModel = mvm;
        _repository = repository;
        _sheetInfo = pylonSheetInfo;
        _viewOfPylon = pylonView;

        _dimensionBaseService = dimensionBaseService;
        _dimSegmentsService = new DimensionSegmentsService(pylonView.ViewElement);
        _gridEndsService = new GridEndsService(_viewOfPylon.ViewElement, dimensionBaseService);
        _dimCreationService = new DimensionCreationService(mvm, repository, pylonView, _dimensionBaseService);
    }

    private bool CreateVerticalDimensions(bool refsForTop, Element pylon, List<Grid> grids) {
        //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
        var rebarFinder = _viewModel.RebarFinder;
        var skeletonParentRebar = _sheetInfo.RebarInfo.SkeletonParentRebar;
        if(skeletonParentRebar is null) { return false; }


        var dimensionLineHostRef = refsForTop ? skeletonParentRebar : pylon;

        var view = _viewOfPylon.ViewElement;


        var refArrayFormworkFront = _dimensionBaseService.GetDimensionRefs(pylon as FamilyInstance, ["фронт", "край"]);

        // Указывает нужно ли будет делать длинные оси снизу (сделано здесь, чтобы не выполнять запрос дважды)
        var vertDimensionsForEdit = new List<Dimension>();
        var longGridsWillBeNeeded = false;

        var topSmallOffset = new DimensionLineOffsetOption(dimensionLineHostRef, DirectionType.Top, 0.5);
        var bottomSmallOffset = new DimensionLineOffsetOption(dimensionLineHostRef, DirectionType.Bottom, 0.5);
        if(refsForTop) {
            // Когда все Гэшки
            if(_sheetInfo.RebarInfo.AllRebarAreL) {
                vertDimensionsForEdit.Add(
                    _dimCreationService.CreateDimension(skeletonParentRebar, bottomSmallOffset, ["низ", "фронт", "край"], 
                                                        refArrayFormworkFront, false));
                longGridsWillBeNeeded = true;
            } else if(_sheetInfo.RebarInfo.HasLRebar) {
                // Когда Гэшки с одной стороны
                if(rebarFinder.DirectionHasLRebar(view, _sheetInfo.ProjectSection, DirectionType.Top)) {
                    vertDimensionsForEdit.Add(
                        _dimCreationService.CreateDimension(skeletonParentRebar, topSmallOffset, 
                                                            ["низ", "фронт", "край"],
                                                            refArrayFormworkFront, false));
                    vertDimensionsForEdit.Add(
                        _dimCreationService.CreateDimension(skeletonParentRebar, bottomSmallOffset, 
                                                            ["верх", "фронт", "край"],
                                                            refArrayFormworkFront, false));
                } else {
                    vertDimensionsForEdit.Add(
                        _dimCreationService.CreateDimension(skeletonParentRebar, topSmallOffset, 
                                                            ["верх", "фронт", "край"],
                                                            refArrayFormworkFront, false));
                    vertDimensionsForEdit.Add(
                        _dimCreationService.CreateDimension(skeletonParentRebar, bottomSmallOffset, 
                                                            ["низ", "фронт", "край"],
                                                            refArrayFormworkFront, false));
                    longGridsWillBeNeeded = true;
                }
            }
            // Когда Гэшек нет вообще
            if(!_sheetInfo.RebarInfo.HasLRebar) {
                vertDimensionsForEdit.Add(
                    _dimCreationService.CreateDimension(skeletonParentRebar, bottomSmallOffset, 
                                                        ["верх", "фронт", "край"],
                                                        refArrayFormworkFront, false));
            }
        } else {
            vertDimensionsForEdit.Add(
                _dimCreationService.CreateDimension(skeletonParentRebar, bottomSmallOffset, ["низ", "фронт", "край"], 
                                                    refArrayFormworkFront, false));
        }

        // Изменяем размер, передвигая текст у крайних сегментов для корректного отображения
        foreach(var dimension in vertDimensionsForEdit) {
            _dimSegmentsService.EditEdgeDimensionSegments(dimension,
                                                          _dimSegmentsService.VertSmallUpDirectDimTextOffset,
                                                          _dimSegmentsService.VertSmallUpInvertedDimTextOffset);
        }

        // Определим отступ для размерной линии общего размера по опалубке (если есть верт оси, то будет дальше)
        var formworkFrontDimensionLineOffset = 1.0;
        // Размеры по осям
        if(grids.Count > 0 && _dimensionBaseService.GetDimensionRefs(grids, XYZ.BasisY).Size > 0) {
            // Размер по ФРОНТУ опалубка + оси (положение сверху 1)

            var offset = new DimensionLineOffsetOption(dimensionLineHostRef, DirectionType.Bottom, 1);
            _dimCreationService.CreateDimension(grids, XYZ.BasisY, offset, refArrayFormworkFront);

            formworkFrontDimensionLineOffset = 1.5;
        }

        // Размер по ФРОНТУ опалубка (положение снизу 1.5)
        var formworkFrontDimOffset = new DimensionLineOffsetOption(dimensionLineHostRef, DirectionType.Bottom,
                                                                   formworkFrontDimensionLineOffset);
        _dimCreationService.CreateDimension(refArrayFormworkFront, formworkFrontDimOffset);

        return longGridsWillBeNeeded;
    }


    private void CreateHorizontalDimensions(bool refsForTop, Element pylon, List<Grid> grids, bool longGridsWillBeNeeded) {
        //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
        var view = _viewOfPylon.ViewElement;
        var skeletonParentRebar = _sheetInfo.RebarInfo.SkeletonParentRebar;
        if(skeletonParentRebar is null) { return; }

        string rebarPart = refsForTop ? "верх" : "низ";
        // Получаем референсы для размеров по крайним плоскостям пилона
        var refArrayFormworkSide = _dimensionBaseService.GetDimensionRefs(pylon as FamilyInstance,
                                                                          ["торец", "край"]);
        // Размер по ТОРЦУ опалубка + армирование (положение справа ближнее)
        ReferenceArray refArraySide = default;
        // Если первый ряд - Гэшки, то берем по нижней плоскости
        if(_sheetInfo.RebarInfo.FirstLRebarParamValue) {
            refArraySide = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar,
                                                                  ["низ", "торец", "_1_"],
                                                                  oldRefArray: refArrayFormworkSide);
        } else {
            refArraySide = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar,
                                                                  [rebarPart, "торец", "_1_"],
                                                                  oldRefArray: refArrayFormworkSide);
        }
        // Если второй ряд - Гэшки, то берем по нижней плоскости
        if(_sheetInfo.RebarInfo.SecondLRebarParamValue) {
            refArraySide = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar,
                                                                  ["низ", "торец", "_2_"],
                                                                  oldRefArray: refArraySide);
        } else {
            refArraySide = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar,
                                                                  [rebarPart, "торец", "_2_"],
                                                                  oldRefArray: refArraySide);
        }
        var formworkRebarDimOffset = new DimensionLineOffsetOption(pylon, DirectionType.Right, 0.6);
        var formworkRebarDimSide = _dimCreationService.CreateDimension(refArraySide, formworkRebarDimOffset, false);
        // Изменяем размер, передвигая текст у крайних сегментов для корректного отображения
        _dimSegmentsService.EditEdgeDimensionSegments(formworkRebarDimSide,
                                                      _dimSegmentsService.HorizSmallUpDirectDimTextOffset,
                                                      _dimSegmentsService.HorizSmallUpInvertedDimTextOffset);

        // Размер по ТОРЦУ опалубка (положение справа дальнее)
        var formworkSideDimOffset = new DimensionLineOffsetOption(pylon, DirectionType.Right, 1);
        _dimCreationService.CreateDimension(refArrayFormworkSide, formworkSideDimOffset);

        // Если на виде есть оси, то создаем размер
        if(grids.Count > 0) {
            // Размер по ТОРЦУ опалубка + оси (положение слева дальнее)
            var offset = new DimensionLineOffsetOption(pylon, DirectionType.Left, 1);
            _dimCreationService.CreateDimension(grids, XYZ.BasisX, offset,  refArrayFormworkSide);
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
    }

    internal void TryCreateDimensions(bool refsForTop, bool pylonFromTop) {
        var doc = _repository.Document;
        var view = _viewOfPylon.ViewElement;
        
        try {
            var grids = new FilteredElementCollector(doc, view.Id)
                .OfCategory(BuiltInCategory.OST_Grids)
                .Cast<Grid>()
                .ToList();

            // Определяем относительно чего нужно строить размерные линии - каркаса или пилона
            var pylon = pylonFromTop ?  _sheetInfo.HostElems.Last() : _sheetInfo.HostElems.First();

            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            bool longGridsWillBeNeeded = CreateVerticalDimensions(refsForTop, pylon, grids);

            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            CreateHorizontalDimensions(refsForTop, pylon, grids, longGridsWillBeNeeded);
        } catch(Exception) { }
    }
}
