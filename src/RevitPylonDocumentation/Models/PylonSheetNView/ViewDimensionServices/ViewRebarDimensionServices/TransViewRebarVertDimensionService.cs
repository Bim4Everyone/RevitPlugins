using System;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.Services;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;

internal class TransViewRebarVertDimensionService {
    private readonly CreationSettings _settings;
    private readonly Document _doc;
    private readonly PylonSheetInfo _sheetInfo;
    private readonly PylonView _viewOfPylon;

    private readonly DimensionBaseService _dimensionBaseService;
    private readonly DimensionCreationService _dimCreationService;

    internal TransViewRebarVertDimensionService(CreationSettings settings, Document document,
                                                PylonSheetInfo pylonSheetInfo, PylonView pylonView,
                                                DimensionBaseService dimensionBaseService) {
        _settings = settings;
        _doc = document;
        _sheetInfo = pylonSheetInfo;
        _viewOfPylon = pylonView;

        _dimensionBaseService = dimensionBaseService;
        _dimCreationService = new DimensionCreationService(settings, document, pylonView, _dimensionBaseService);
    }

    internal void TryCreateDimensions(bool onTopOfRebar, bool dimensionLineFromPylon) {
        try {
            var skeletonParentRebar = _sheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) { return; }

            var pylon = _sheetInfo.HostElems.First();
            var dimensionLineHostRef = dimensionLineFromPylon ? pylon : skeletonParentRebar;
            var edgeBottomFrontRefArray = _dimensionBaseService.GetDimensionRefs(skeletonParentRebar, 
                                                                                 ["низ", "фронт", "край"]);

            var topSmallOffset = new DimensionLineOffsetOption(dimensionLineHostRef, 
                                                               DirectionType.Top, 
                                                               0.5);
            var topBigOffset = new DimensionLineOffsetOption(dimensionLineHostRef, 
                                                             DirectionType.Top, 
                                                             1);
            var bottomSmallOffset = new DimensionLineOffsetOption(dimensionLineHostRef, 
                                                                  DirectionType.Bottom, 
                                                                  0.5);
            var bottomBigOffset = new DimensionLineOffsetOption(dimensionLineHostRef, 
                                                                DirectionType.Bottom, 
                                                                1);

            if(onTopOfRebar) {
                if(_sheetInfo.RebarInfo.AllRebarAreL) {
                    _dimCreationService.CreateDimension(skeletonParentRebar, bottomSmallOffset, ["низ", "фронт"]);
                    _dimCreationService.CreateDimension(skeletonParentRebar, bottomBigOffset, ["низ", "фронт", "край"]);
                } else if(_sheetInfo.RebarInfo.HasLRebar) {
                    var rebarFinder = _sheetInfo.RebarFinder;
                    var view = _viewOfPylon.ViewElement;
                    if(rebarFinder.DirectionHasLRebar(view, _sheetInfo.ProjectSection, DirectionType.Top)) {
                        _dimCreationService.CreateDimension(skeletonParentRebar, 
                                                            topSmallOffset, 
                                                            ["низ", "фронт"]);
                        _dimCreationService.CreateDimension(skeletonParentRebar, 
                                                            topBigOffset, 
                                                            ["низ", "фронт", "край"]);
                        _dimCreationService.CreateDimension(skeletonParentRebar, 
                                                            bottomSmallOffset, 
                                                            ["верх", "фронт"], 
                                                            edgeBottomFrontRefArray);
                        _dimCreationService.CreateDimension(skeletonParentRebar, 
                                                            bottomBigOffset, 
                                                            ["верх", "фронт", "край"]);
                    } else {
                        _dimCreationService.CreateDimension(skeletonParentRebar, 
                                                            topSmallOffset, 
                                                            ["верх", "фронт"], 
                                                            edgeBottomFrontRefArray);
                        _dimCreationService.CreateDimension(skeletonParentRebar, 
                                                            topBigOffset, 
                                                            ["верх", "фронт", "край"]);
                        _dimCreationService.CreateDimension(skeletonParentRebar, 
                                                            bottomSmallOffset, 
                                                            ["низ", "фронт"]);
                        _dimCreationService.CreateDimension(skeletonParentRebar, 
                                                            bottomBigOffset, 
                                                            ["низ", "фронт", "край"]);
                    }
                } else {
                    _dimCreationService.CreateDimension(skeletonParentRebar, 
                                                        bottomSmallOffset, 
                                                        ["верх", "фронт"], 
                                                        edgeBottomFrontRefArray);
                    _dimCreationService.CreateDimension(skeletonParentRebar, 
                                                        bottomBigOffset, 
                                                        ["низ", "фронт", "край"]);
                }
            } else {
                _dimCreationService.CreateDimension(skeletonParentRebar, 
                                                    bottomSmallOffset, 
                                                    ["низ", "фронт"]);
                _dimCreationService.CreateDimension(skeletonParentRebar, 
                                                    bottomBigOffset, 
                                                    ["низ", "фронт", "край"]);
            }
        } catch(Exception) { }
    }
}
