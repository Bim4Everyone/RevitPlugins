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

    private bool _longGridsWillBeNeeded;

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


    /// <summary>
    /// Создает вертикальные размеры по опалубке и армированию
    /// </summary>
    private List<Dimension> CreateVertDimsForFormNRebar(bool refsForTop, Element skeletonParentRebar,
                                                        Element dimensionLineHostRef,
                                                        ReferenceArray refArrayFormworkFront) {
        var vertDimensionsForEdit = new List<Dimension>();
        var topSmallOffset = new DimensionLineOffsetOption(dimensionLineHostRef, DirectionType.Top, 0.5);
        var bottomSmallOffset = new DimensionLineOffsetOption(dimensionLineHostRef, DirectionType.Bottom, 0.5);

        if(refsForTop) {
            // Если нужно брать опорные плоскости по верху
            if(_sheetInfo.RebarInfo.AllRebarAreL) {
                // Когда все Гэшки
                vertDimensionsForEdit.Add(
                    _dimCreationService.CreateDimension(
                        skeletonParentRebar,
                        bottomSmallOffset,
                        ["низ", "фронт", "край"],
                        refArrayFormworkFront,
                        false
                    )
                );
                _longGridsWillBeNeeded = true;
            } else if(_sheetInfo.RebarInfo.HasLRebar) {
                // Когда Гэшки с одной стороны
                if(_viewModel.RebarFinder.DirectionHasLRebar(_viewOfPylon.ViewElement, _sheetInfo.ProjectSection,
                                                             DirectionType.Top)) {
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
                    _longGridsWillBeNeeded = true;
                }
            } else {
                // Когда Гэшек нет вообще
                vertDimensionsForEdit.Add(
                    _dimCreationService.CreateDimension(skeletonParentRebar,
                                                        bottomSmallOffset,
                                                        ["верх", "фронт", "край"],
                                                        refArrayFormworkFront,
                                                        false)
                );
            }
        } else {
            // Если нужно брать опорные плоскости по низу
            vertDimensionsForEdit.Add(
                _dimCreationService.CreateDimension(skeletonParentRebar, 
                                                    bottomSmallOffset,
                                                    ["низ", "фронт", "край"],
                                                    refArrayFormworkFront,
                                                    false)
            );
        }
        return vertDimensionsForEdit;
    }

    /// <summary>
    /// Редактирует крайние сегменты вертикальных размеров
    /// </summary>
    private void EditVertDimensionSegments(List<Dimension> vertDimensionsForEdit) {
        foreach(var dimension in vertDimensionsForEdit) {
            _dimSegmentsService.EditEdgeDimensionSegments(dimension,
                                                          _dimSegmentsService.VertSmallUpDirectDimTextOffset,
                                                          _dimSegmentsService.VertSmallUpInvertedDimTextOffset);
        }
    }

    /// <summary>
    /// Создает вертикальные размеры по опалубке и осям
    /// </summary>
    private double CreateVertDimensionByFormNGrids(List<Grid> grids, Element dimensionLineHostRef,
                                                   ReferenceArray refArrayFormworkFront) {
        // Определим отступ для размерной линии общего размера по опалубке (если есть верт оси, то будет дальше)
        double formworkFrontDimensionLineOffset = 1.0;
        // Размеры по осям
        if(grids.Count > 0 && _dimensionBaseService.GetDimensionRefs(grids, XYZ.BasisY).Size > 0) {
            // Размер по ФРОНТУ опалубка + оси (положение сверху 1)

            var offset = new DimensionLineOffsetOption(dimensionLineHostRef, DirectionType.Bottom, 1);
            _dimCreationService.CreateDimension(grids, XYZ.BasisY, offset, refArrayFormworkFront);

            formworkFrontDimensionLineOffset = 1.5;
        }
        return formworkFrontDimensionLineOffset;
    }

    /// <summary>
    /// Создает вертикальные размеры по опалубке
    /// </summary>
    private void CreateVertDimensionByForm(Element dimensionLineHostRef, 
                                           double formworkFrontDimensionLineOffset,
                                           ReferenceArray refArrayFormworkFront) {
        // Размер по ФРОНТУ опалубка (положение снизу 1.5)
        var formworkFrontDimOffset = new DimensionLineOffsetOption(dimensionLineHostRef, DirectionType.Bottom,
                                                                   formworkFrontDimensionLineOffset);
        _dimCreationService.CreateDimension(refArrayFormworkFront, formworkFrontDimOffset);
    }


    /// <summary>
    /// Получает опорные плоскости для горизонтальных размеров
    /// </summary>
    private ReferenceArray GetHorizRefs(FamilyInstance skeletonParentRebar, ReferenceArray refArrayFormworkSide, 
                                        string rebarPart) {
        ReferenceArray refArraySide = refArrayFormworkSide;

        // Если первый ряд - Гэшки, то берем по нижней плоскости
        if(_sheetInfo.RebarInfo.FirstLRebarParamValue) {
            refArraySide = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar,
                                                                  ["низ", "торец", "_1_"],
                                                                  oldRefArray: refArraySide);
        } else {
            refArraySide = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar, 
                                                                  [rebarPart, "торец", "_1_"],
                                                                  oldRefArray: refArraySide);
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
        return refArraySide;
    }

    /// <summary>
    /// Создает горизонтальные размеры по опалубке и армированию
    /// </summary>
    private void CreateHorizDimensionForFormNRebar(Element pylon, ReferenceArray refArraySide) {
        var formworkRebarDimOffset = new DimensionLineOffsetOption(pylon, DirectionType.Right, 0.6);
        var formworkRebarDimSide = _dimCreationService.CreateDimension(refArraySide, formworkRebarDimOffset, false);

        _dimSegmentsService.EditEdgeDimensionSegments(formworkRebarDimSide,
                                                      _dimSegmentsService.HorizSmallUpDirectDimTextOffset,
                                                      _dimSegmentsService.HorizSmallUpInvertedDimTextOffset);
    }

    /// <summary>
    /// Создает горизонтальные размеры по опалубке и осям
    /// </summary>
    private void CreateHorizDimensionByFormNGrids(Element pylon, List<Grid> grids, ReferenceArray refArrayFormworkSide) {
        var offset = new DimensionLineOffsetOption(pylon, DirectionType.Left, 1);
        _dimCreationService.CreateDimension(grids, XYZ.BasisX, offset, refArrayFormworkSide);
    }

    /// <summary>
    /// Создает горизонтальные размеры по опалубке
    /// </summary>
    private void CreateHorizDimensionForForm(Element pylon, ReferenceArray refArrayFormworkSide) {
        var formworkSideDimOffset = new DimensionLineOffsetOption(pylon, DirectionType.Right, 1);
        _dimCreationService.CreateDimension(refArrayFormworkSide, formworkSideDimOffset);
    }


    /// <summary>
    /// Редактирует положения концов осей на виде
    /// </summary>
    private void EditGridEnds(Element pylon, List<Grid> grids, bool longGridsWillBeNeeded) {
        var transverseViewGridOffsets = new OffsetOption {
            LeftOffset = 1.5,
            RightOffset = 0.3,
            TopOffset = 0.2,
            BottomOffset = 1.6
        };

        if(longGridsWillBeNeeded) {
            transverseViewGridOffsets.BottomOffset = 3.0;
        }
        _gridEndsService.EditGridEnds(pylon, grids, transverseViewGridOffsets);
    }



    /// <summary>
    /// Создает размеры по вертикальным опорным плоскостям
    /// </summary>
    private void CreateVerticalDimensions(bool refsForTop, Element pylon, List<Grid> grids) {
        var skeletonParentRebar = _sheetInfo.RebarInfo.SkeletonParentRebar;
        if(skeletonParentRebar is null) { return; }

        var dimensionLineHostRef = refsForTop ? skeletonParentRebar : pylon;
        var refArrayFormworkFront = _dimensionBaseService.GetDimensionRefs(pylon as FamilyInstance, ["фронт", "край"]);

        // Размер по ФРОНТУ опалубка + армирование(положение сверху/снизу 0.5)
        var vertDimsForEdit = CreateVertDimsForFormNRebar(refsForTop, skeletonParentRebar, dimensionLineHostRef,
                                                          refArrayFormworkFront);

        // Изменяем размер, передвигая текст у крайних сегментов для корректного отображения
        EditVertDimensionSegments(vertDimsForEdit);

        // Размер по ФРОНТУ опалубка + оси (положение снизу 1)
        // Положение размера по опалубке зависит от того будет ли установлен этот размер
        double formworkFrontDimLineOffset = CreateVertDimensionByFormNGrids(grids,
                                                                            dimensionLineHostRef,
                                                                            refArrayFormworkFront);
        // Размер по ФРОНТУ опалубка (положение снизу 1.5)
        CreateVertDimensionByForm(dimensionLineHostRef, formworkFrontDimLineOffset, refArrayFormworkFront);
    }


    /// <summary>
    /// Создает размеры по горизонтальным опорным плоскостям
    /// </summary>
    private void CreateHorizontalDimensions(bool refsForTop, Element pylon, List<Grid> grids) {
        var skeletonParentRebar = _sheetInfo.RebarInfo.SkeletonParentRebar;
        if(skeletonParentRebar is null) { return; }

        string rebarPart = refsForTop ? "верх" : "низ";
        var refArrayFormworkSide = _dimensionBaseService.GetDimensionRefs(pylon as FamilyInstance, ["торец", "край"]);
        var refArraySide = GetHorizRefs(skeletonParentRebar, refArrayFormworkSide, rebarPart);
        
        // Размер по ТОРЦУ опалубка (положение справа 0.6)
        CreateHorizDimensionForFormNRebar(pylon, refArraySide);

        // Размер по ТОРЦУ опалубка (положение справа 1)
        CreateHorizDimensionForForm(pylon, refArrayFormworkSide);

        // Размер по ТОРЦУ опалубка + оси (положение слева 1)
        if(grids.Count > 0) {
            CreateHorizDimensionByFormNGrids(pylon, grids, refArrayFormworkSide);
        }
    }


    internal void TryCreateDimensions(bool refsForTop, bool pylonFromTop) {
        _longGridsWillBeNeeded = false;
        try {
            var grids = _repository.GridsInView(_viewOfPylon.ViewElement);

            // Определяем относительно какого пилона нужны размеры - верхнего или нижнего  
            var pylon = pylonFromTop ?  _sheetInfo.HostElems.Last() : _sheetInfo.HostElems.First();

            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            CreateVerticalDimensions(refsForTop, pylon, grids);

            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            CreateHorizontalDimensions(refsForTop, pylon, grids);

            // Корректируем концы осей, чтобы размеры смотрелись корректнее
            EditGridEnds(pylon, grids, _longGridsWillBeNeeded);
        } catch(Exception) { }
    }
}
