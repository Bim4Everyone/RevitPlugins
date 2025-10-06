using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

using Grid = Autodesk.Revit.DB.Grid;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewFormDimensionServices;
internal class TransViewHorizDimensionService {

    private readonly MainViewModel _viewModel;
    private readonly RevitRepository _repository;
    private readonly PylonSheetInfo _sheetInfo;
    private readonly PylonView _viewOfPylon;

    private readonly DimensionBaseService _dimensionBaseService;
    private readonly DimensionSegmentsService _dimSegmentsService;
    private readonly DimensionCreationService _dimCreationService;

    internal TransViewHorizDimensionService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo,
                                           PylonView pylonView, DimensionBaseService dimensionBaseService) {
        _viewModel = mvm;
        _repository = repository;
        _sheetInfo = pylonSheetInfo;
        _viewOfPylon = pylonView;

        _dimensionBaseService = dimensionBaseService;
        _dimSegmentsService = new DimensionSegmentsService(pylonView.ViewElement);
        _dimCreationService = new DimensionCreationService(mvm, repository, pylonView, _dimensionBaseService);
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


    internal void TryCreateDimensions(bool refsForTop, Element pylon, List<Grid> grids) {
        try {
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
        } catch(Exception) { }
    }
}
