using System;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewRebarMarkServices;
using RevitPylonDocumentation.Models.Services;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class GeneralViewRebarAnnotCreator : ViewAnnotationCreator {
    public GeneralViewRebarAnnotCreator(CreationSettings settings, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                          PylonView pylonView) 
        : base(settings, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewAnnotations() {
        var view = ViewOfPylon.ViewElement;
        var doc = view.Document;
        var dimensionBaseService = new DimensionBaseService(view, SheetInfo.ParamValService);

        // Пытаемся создать размеры на виде
        try {
            var rebarFinder = SheetInfo.RebarFinder;
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }
            // Получаем родительское семейство хомутов на виде
            var clampsParentRebars = rebarFinder.GetClampsParentRebars(view, SheetInfo.ProjectSection);
            if(clampsParentRebars is null) {
                return;
            }

            var dimensionService = new GeneralViewRebarDimensionService(Settings, doc, SheetInfo, ViewOfPylon, 
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
            var markService = new GeneralViewRebarMarkService(Settings, doc, SheetInfo, ViewOfPylon);
            markService.TryCreateClampMarks(true);
        } catch(Exception) { }
    }
}
