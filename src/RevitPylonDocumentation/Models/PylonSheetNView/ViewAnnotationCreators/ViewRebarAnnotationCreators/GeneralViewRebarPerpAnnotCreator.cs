using System;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewRebarMarkServices;
using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class GeneralViewRebarPerpAnnotCreator : ViewAnnotationCreator {
    internal GeneralViewRebarPerpAnnotCreator(MainViewModel mvm, RevitRepository repository, 
                                              PylonSheetInfo pylonSheetInfo, PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewAnnotations() {
        var dimensionService = new GeneralViewRebarPerpDimensionService(ViewModel, Repository, SheetInfo, ViewOfPylon);
        var dimensionBaseService = new DimensionBaseService(ViewOfPylon.ViewElement, ViewModel.ParamValService);

        // Пытаемся создать размеры на виде
        try {
            var rebarFinder = ViewModel.RebarFinder;
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }
            // Создаем размеры по вертикальным стержням снизу и сверху (если нужно)
            dimensionService.TryCreateTopEdgeRebarDimensions(skeletonParentRebar, dimensionBaseService);
            dimensionService.TryCreateBottomEdgeRebarDimensions(skeletonParentRebar, dimensionBaseService);

            // Размер по Гэшке сбоку
            dimensionService.TryCreateVertLRebarDimension(skeletonParentRebar, dimensionBaseService);
            // Размер по Гэшке сверху
            dimensionService.TryCreateHorizLRebarDimension(skeletonParentRebar, dimensionBaseService);
            // Создаем размеры по изгибам вертикальных стержней-бутылок
            dimensionService.TryCreateAdditionalDimensions(skeletonParentRebar, dimensionBaseService);
        } catch(Exception) { }

        // Пытаемся создать марки на виде
        try {
            var markService = new GeneralViewRebarMarkService(ViewModel, Repository, SheetInfo, ViewOfPylon);
            markService.TryCreateVerticalBarMarks();
            markService.TryCreateClampMarks(false);
        } catch(Exception) { }
    }
}
