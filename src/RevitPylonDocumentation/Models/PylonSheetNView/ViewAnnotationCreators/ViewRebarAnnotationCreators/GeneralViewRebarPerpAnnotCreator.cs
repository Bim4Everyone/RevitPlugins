using System;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewRebarMarkServices;
using RevitPylonDocumentation.Models.Services;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class GeneralViewRebarPerpAnnotCreator : ViewAnnotationCreator {
    public GeneralViewRebarPerpAnnotCreator(CreationSettings settings, RevitRepository repository, 
                                              PylonSheetInfo pylonSheetInfo, PylonView pylonView) 
        : base(settings, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewAnnotations() {
        var view = ViewOfPylon.ViewElement;
        var doc = view.Document;
        var dimensionBaseService = new DimensionBaseService(ViewOfPylon.ViewElement, SheetInfo.ParamValService);

        // Пытаемся создать размеры на виде
        try {
            var dimensionService = new GeneralViewRebarPerpDimensionService(Settings, doc, SheetInfo, 
                                                                            ViewOfPylon, dimensionBaseService);
            var skeletonParentRebar = SheetInfo.RebarInfo.SkeletonParentRebar;
            if(skeletonParentRebar is null) {
                return;
            }
            // Создаем размеры по вертикальным стержням снизу и сверху (если нужно)
            dimensionService.TryCreateTopEdgeRebarDimensions(skeletonParentRebar);
            dimensionService.TryCreateBottomEdgeRebarDimensions(skeletonParentRebar);

            // Размер по Гэшке сбоку
            dimensionService.TryCreateVertLRebarDimension(skeletonParentRebar);
            // Размер по Гэшке сверху
            dimensionService.TryCreateHorizLRebarDimension(skeletonParentRebar);
            // Создаем размеры по изгибам вертикальных стержней-бутылок
            dimensionService.TryCreateAdditionalDimensions(skeletonParentRebar);
        } catch(Exception) { }

        // Пытаемся создать марки на виде
        try {
            var markService = new GeneralViewRebarMarkService(Settings, doc, SheetInfo, ViewOfPylon);
            markService.TryCreateVerticalBarMarks();
            markService.TryCreateClampMarks(false);
        } catch(Exception) { }
    }
}
