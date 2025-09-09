using System;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewFormDimensionServices;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewFormMarkServices;
using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class GeneralViewPerpAnnotCreator : ViewAnnotationCreator {
    internal GeneralViewPerpAnnotCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                         PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewAnnotations() {
        var view = ViewOfPylon.ViewElement;
        var dimensionBaseService = new DimensionBaseService(view, ViewModel.ParamValService);

        // Пытаемся создать размеры на виде
        try {
            // Получаем родительское семейство вертикальных стержней на виде
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }
            // Получаем родительское семейство хомутов на виде
            var rebarFinder = ViewModel.RebarFinder;
            var clampsParentRebars = rebarFinder.GetClampsParentRebars(view, SheetInfo.ProjectSection);
            if(clampsParentRebars is null) {
                return;
            }
            // Получаем оси на виде
            var grids = Repository.GridsInView(view);

            var dimensionService = new GeneralViewDimensionService(ViewModel, Repository, SheetInfo, ViewOfPylon);
            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            dimensionService.TryCreatePylonDimensions(skeletonParentRebar, grids, dimensionBaseService, false);
            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            dimensionService.TryCreatePylonDimensions(SheetInfo.HostElems, dimensionBaseService, true);
            dimensionService.TryCreateTopAdditionalDimensions(skeletonParentRebar, dimensionBaseService, true);
            dimensionService.TryCreateClampsDimensions(clampsParentRebars, dimensionBaseService, true);
            dimensionService.TryCreateHorizLRebarDimension(dimensionBaseService);
        } catch(Exception) { }

        // Пытаемся создать марки на виде
        try {
            var markService = new GeneralViewMarkService(ViewModel, Repository, SheetInfo, ViewOfPylon);
            markService.TryCreatePylonElevMark(SheetInfo.HostElems, dimensionBaseService);
            markService.TryCreateSkeletonMark();
            markService.TryCreateAdditionalMark(true);
            markService.TryCreateLowerBreakLines(true);
            markService.TryCreateUpperBreakLines();
            markService.TryCreateMiddleBreakLines(true);
            markService.TryCreateConcretingSeams();
        } catch(Exception) { }
    }
}
