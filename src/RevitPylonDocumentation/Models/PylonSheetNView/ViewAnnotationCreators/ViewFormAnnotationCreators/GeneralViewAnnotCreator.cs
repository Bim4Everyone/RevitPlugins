using System;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewFormDimensionServices;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewFormMarkServices;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class GeneralViewAnnotCreator : ViewAnnotationCreator {
    public GeneralViewAnnotCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
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
            dimensionService.TryCreatePylonDimensions(skeletonParentRebar, grids, dimensionBaseService, true);
            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            dimensionService.TryCreatePylonDimensions(SheetInfo.HostElems, dimensionBaseService);
            dimensionService.TryCreateTopAdditionalDimensions(skeletonParentRebar, dimensionBaseService);
            dimensionService.TryCreateClampsDimensions(clampsParentRebars, dimensionBaseService, true);
            dimensionService.TryCreateClampsDimensions(clampsParentRebars, dimensionBaseService, false);
        } catch(Exception) { }

        // Пытаемся создать марки на виде
        try {
            var markService = new GeneralViewMarkService(ViewModel, Repository, SheetInfo, ViewOfPylon);
            markService.TryCreatePylonElevMark(SheetInfo.HostElems, dimensionBaseService);
            markService.TryCreateSkeletonMark();
            markService.TryCreateClampMarks(true);
            markService.TryCreateAdditionalMark();
            markService.TryCreateLowerBreakLines();
            markService.TryCreateUpperBreakLines();
            markService.TryCreateMiddleBreakLines();
            markService.TryCreateConcretingSeams();
        } catch(Exception) { }
    }
}
