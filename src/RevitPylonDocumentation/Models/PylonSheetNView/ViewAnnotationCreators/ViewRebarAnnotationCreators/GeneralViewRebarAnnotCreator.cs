using System;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewRebarMarkServices;
using RevitPylonDocumentation.Models.Services;
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
            // Получаем родительское семейство хомутов на виде
            var clampsParentRebars = rebarFinder.GetClampsParentRebars(view, SheetInfo.ProjectSection);
            if(clampsParentRebars is null) {
                return;
            }

            var dimensionService = new GeneralViewRebarDimensionService(ViewModel, Repository, SheetInfo, ViewOfPylon, 
                                                                        dimensionBaseService);
            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            dimensionService.TryCreateAllTopRebarDimensions(skeletonParentRebar);
            dimensionService.TryCreateAllBottomRebarDimensions(skeletonParentRebar);
            dimensionService.TryCreateEdgeRebarDimensions(skeletonParentRebar);

            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            dimensionService.TryCreateClampsDimensions(clampsParentRebars, skeletonParentRebar);
        } catch(Exception) { }

        // Пытаемся создать марки на виде
        try {
            var markService = new GeneralViewRebarMarkService(ViewModel, Repository, SheetInfo, ViewOfPylon);
            markService.TryCreateClampMarks(true);
        } catch(Exception) { }
    }
}
