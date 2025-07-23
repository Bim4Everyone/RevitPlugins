using System;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class GeneralViewRebarPerpAnnotCreator : ViewAnnotationCreator {
    internal GeneralViewRebarPerpAnnotCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }


    public override void TryCreateViewAnnotations() {

        var doc = Repository.Document;
        var view = SheetInfo.GeneralViewPerpendicularRebar.ViewElement;
        var dimensionBaseService = new DimensionBaseService(view, ViewModel.ParamValService);

        var dimensionService = new GeneralViewRebarPerpDimensionService(ViewModel, Repository, SheetInfo, ViewOfPylon);

        // Пытаемся создать размеры на виде
        try {
            var rebarFinder = ViewModel.RebarFinder;
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }

            dimensionService.CreateTopEdgeRebarDimensions(skeletonParentRebar, dimensionBaseService);
            dimensionService.CreateBottomEdgeRebarDimensions(skeletonParentRebar, dimensionBaseService);


            var plateDimensionOffsetType = DimensionOffsetType.Right;
            // Будем ставить размерную цепочку по дефолту справа
            // Слева будем ставить только если есть Г-образный стержень (но не все) и он справа
            if(SheetInfo.RebarInfo.HasLRebar && dimensionService.LRebarIsRight(view, rebarFinder)) {
                plateDimensionOffsetType = DimensionOffsetType.Left;
            }

            var plates = rebarFinder.GetSimpleRebars(view, SheetInfo.ProjectSection, 2001);
            dimensionService.CreatePlateDimensions(skeletonParentRebar, plates, plateDimensionOffsetType, dimensionBaseService);


            if(!SheetInfo.RebarInfo.AllRebarAreL && SheetInfo.RebarInfo.HasLRebar) {
                var rebarDimensionOffsetType = plateDimensionOffsetType == 
                    DimensionOffsetType.Left ? DimensionOffsetType.Right : DimensionOffsetType.Left;
                dimensionService.CreateLRebarDimension(skeletonParentRebar, rebarDimensionOffsetType, dimensionBaseService);
            }

            dimensionService.CreateAdditionalDimensions(skeletonParentRebar, dimensionBaseService);
        } catch(Exception) { }

        // Пытаемся создать марки на виде
        try {

        } catch(Exception) { }
    }
}
