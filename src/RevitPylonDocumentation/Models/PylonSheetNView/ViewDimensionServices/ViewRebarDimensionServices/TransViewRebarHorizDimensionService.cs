using System;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.Services;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
internal class TransViewRebarHorizDimensionService {
    private readonly CreationSettings _settings;
    private readonly Document _doc;
    private readonly PylonSheetInfo _sheetInfo;
    private readonly PylonView _viewOfPylon;
    private readonly DimensionBaseService _dimensionBaseService;
    private readonly DimensionCreationService _dimCreationService;

    internal TransViewRebarHorizDimensionService(CreationSettings settings, Document document,
                                                 PylonSheetInfo pylonSheetInfo, PylonView pylonView,
                                                 DimensionBaseService dimensionBaseService) {
        _settings = settings;
        _doc = document;
        _sheetInfo = pylonSheetInfo;
        _viewOfPylon = pylonView;

        _dimensionBaseService = dimensionBaseService;
        _dimCreationService = new DimensionCreationService(settings, document, pylonView, _dimensionBaseService);
    }

    internal void TryCreateDimensions(bool onTopOfRebar) {
        try {
            var skeletonParentRebar = _sheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }

            var pylon = _sheetInfo.HostElems.First();
            var formDimensionLineOffset = 0.5;

            if(onTopOfRebar && !_sheetInfo.RebarInfo.AllRebarAreL) {
                var edgeBottomSideRefArray = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar,
                                                                                    ["низ", "торец", "край"]);
                if(_sheetInfo.RebarInfo.HasLRebar) {
                    var rebarFinder = _sheetInfo.RebarFinder;
                    var view = _viewOfPylon.ViewElement;
                    if(rebarFinder.DirectionHasLRebar(view, _sheetInfo.ProjectSection, DirectionType.Top)
                        && _sheetInfo.RebarInfo.SecondLRebarParamValue) {
                        edgeBottomSideRefArray =
                            _dimensionBaseService.GetDimensionRefs(skeletonParentRebar,
                                                                   ["1", "верх", "торец", "край"],
                                                                   oldRefArray: edgeBottomSideRefArray);
                    } else {
                        edgeBottomSideRefArray =
                            _dimensionBaseService.GetDimensionRefs(skeletonParentRebar,
                                                                   ["2", "верх", "торец", "край"],
                                                                   oldRefArray: edgeBottomSideRefArray);
                    }
                } else {
                    edgeBottomSideRefArray =
                        _dimensionBaseService.GetDimensionRefs(skeletonParentRebar,
                                                               ["верх", "торец", "край"],
                                                               oldRefArray: edgeBottomSideRefArray);
                }

                var edgeBottomSideOffset = new DimensionLineOffsetOption(pylon, DirectionType.Right, 0.5);
                _dimCreationService.CreateDimension(edgeBottomSideRefArray, edgeBottomSideOffset, false);
                formDimensionLineOffset = 1;
            }

            var rebarSideOffset = new DimensionLineOffsetOption(pylon, DirectionType.Right, formDimensionLineOffset);
            _dimCreationService.CreateDimension(skeletonParentRebar, rebarSideOffset, ["низ", "торец", "край"],
                                                needEqualityFormula: _settings.ProjectSettings.DimensionGrouping);
        } catch(Exception) { }
    }
}
