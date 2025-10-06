using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

using Grid = Autodesk.Revit.DB.Grid;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewFormDimensionServices;
internal class TransViewVertDimensionService {
    private readonly MainViewModel _viewModel;
    private readonly RevitRepository _repository;
    private readonly PylonSheetInfo _sheetInfo;
    private readonly PylonView _viewOfPylon;

    private readonly DimensionBaseService _dimensionBaseService;
    private readonly DimensionSegmentsService _dimSegmentsService;
    private readonly DimensionCreationService _dimCreationService;

    private bool _longGridsWillBeNeeded;

    internal TransViewVertDimensionService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
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
    /// Создает размеры по вертикальным опорным плоскостям
    /// </summary>
    internal bool TryCreateDimensions(bool refsForTop, Element pylon, List<Grid> grids) {
        _longGridsWillBeNeeded = false;

        var skeletonParentRebar = _sheetInfo.RebarInfo.SkeletonParentRebar;
        if(skeletonParentRebar is null) { return _longGridsWillBeNeeded; }

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

        return _longGridsWillBeNeeded;
    }
}
