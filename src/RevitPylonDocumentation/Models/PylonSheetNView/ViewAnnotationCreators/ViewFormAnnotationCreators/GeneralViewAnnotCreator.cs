using System;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewFormDimensionServices;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices;
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
            dimensionService.CreatePylonDimensions(skeletonParentRebar, grids, dimensionBaseService);
            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            dimensionService.CreateClampsDimensions(clampsParentRebars, dimensionBaseService);
            dimensionService.CreateTopAdditionalDimensions(skeletonParentRebar, dimensionBaseService);
            dimensionService.CreatePylonDimensions(SheetInfo.HostElems, dimensionBaseService);
        } catch(Exception) { }

        // Пытаемся создать марки на виде
        try {
            var markService = new GeneralViewMarkService(ViewModel, Repository, SheetInfo, ViewOfPylon);
            markService.CreatePylonElevMark(SheetInfo.HostElems, dimensionBaseService);
        } catch(Exception) { }
    }
}
