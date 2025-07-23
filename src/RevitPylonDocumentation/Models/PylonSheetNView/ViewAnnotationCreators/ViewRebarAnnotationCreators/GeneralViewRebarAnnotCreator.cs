using System;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class GeneralViewRebarAnnotCreator : ViewAnnotationCreator {
    internal GeneralViewRebarAnnotCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                          PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewAnnotations() {
        var view = ViewOfPylon.ViewElement;
        var dimensionBaseService = new DimensionBaseService(view, ViewModel.ParamValService);

        // Пытаемся создать размеры на виде
        try {
            var rebarFinder = ViewModel.RebarFinder;
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }

            var dimensionService = new GeneralViewRebarDimensionService(ViewModel, Repository, SheetInfo, ViewOfPylon);
            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            dimensionService.TryCreateAllTopRebarDimensions(skeletonParentRebar, dimensionBaseService);
            dimensionService.TryCreateAllBottomRebarDimensions(skeletonParentRebar, dimensionBaseService);
            dimensionService.TryCreateEdgeRebarDimensions(skeletonParentRebar, dimensionBaseService);

            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            dimensionService.TryCreatePlateDimensions(skeletonParentRebar, DimensionOffsetType.Left, 
                                                      dimensionBaseService);
        } catch(Exception) { }

        // Пытаемся создать марки на виде
        try {

        } catch(Exception) { }
    }
}
