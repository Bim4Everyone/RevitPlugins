using System;
using System.Collections.Generic;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class GeneralViewRebarAnnotCreator : ViewAnnotationCreator {
    internal GeneralViewRebarAnnotCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewAnnotations() {
        var doc = Repository.Document;
        var view = SheetInfo.GeneralViewRebar.ViewElement;
        var dimensionBaseService = new DimensionBaseService(view, ViewModel.ParamValService);


        // Пытаемся создать размеры на виде
        try {
            var rebarFinder = ViewModel.RebarFinder;
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }
            var plates = rebarFinder.GetSimpleRebars(view, SheetInfo.ProjectSection, 2001);

            var dimensionService = new GeneralViewRebarDimensionService(ViewModel, Repository, SheetInfo, ViewOfPylon);
            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            dimensionService.CreateAllTopRebarDimensions(skeletonParentRebar, dimensionBaseService);
            dimensionService.CreateAllBottomRebarDimensions(skeletonParentRebar, dimensionBaseService);
            dimensionService.CreateEdgeRebarDimensions(skeletonParentRebar, dimensionBaseService);

            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            dimensionService.CreatePlateDimensions(skeletonParentRebar, plates, DimensionOffsetType.Left, dimensionBaseService);
        } catch(Exception) { }

        // Пытаемся создать марки на виде
        try {

        } catch(Exception) { }
    }
}
