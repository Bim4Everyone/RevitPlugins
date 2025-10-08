using System;
using System.Linq;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
internal class TransViewRebarHorizDimensionService {
    private readonly MainViewModel _viewModel;
    private readonly RevitRepository _repository;
    private readonly PylonSheetInfo _sheetInfo;
    private readonly PylonView _viewOfPylon;
    private readonly DimensionBaseService _dimensionBaseService;
    private readonly DimensionCreationService _dimCreationService;

    internal TransViewRebarHorizDimensionService(MainViewModel mvm, RevitRepository repository,
                                                 PylonSheetInfo pylonSheetInfo, PylonView pylonView,
                                                 DimensionBaseService dimensionBaseService) {
        _viewModel = mvm;
        _repository = repository;
        _sheetInfo = pylonSheetInfo;
        _viewOfPylon = pylonView;

        _dimensionBaseService = dimensionBaseService;
        _dimCreationService = new DimensionCreationService(mvm, repository, pylonView, _dimensionBaseService);
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
                    var rebarFinder = _viewModel.RebarFinder;
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
            _dimCreationService.CreateDimension(skeletonParentRebar, rebarSideOffset, ["низ", "торец", "край"]);
        } catch(Exception) { }
    }
}
