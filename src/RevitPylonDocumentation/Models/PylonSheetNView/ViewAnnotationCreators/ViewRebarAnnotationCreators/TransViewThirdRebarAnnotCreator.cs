using System;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewRebarMarkServices;
using RevitPylonDocumentation.Models.Services;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class TransViewThirdRebarAnnotCreator : ViewAnnotationCreator {
    public TransViewThirdRebarAnnotCreator(CreationSettings settings, RevitRepository repository, PylonSheetInfo pylonSheetInfo,
                                           PylonView pylonView)
        : base(settings, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewAnnotations() {
        var view = ViewOfPylon.ViewElement;
        var doc = view.Document;
        var dimensionBaseService = new DimensionBaseService(ViewOfPylon.ViewElement, SheetInfo.ParamValService);

        // Пытаемся создать размеры на виде
        try {
            bool refsForTop = SheetInfo.RebarInfo.SkeletonParentRebarForParking ? false : true;

            // Вертикальные размеры
            var vertDimensionService = new TransViewRebarVertDimensionService(Settings, doc, SheetInfo,
                                                                              ViewOfPylon, dimensionBaseService);
            vertDimensionService.TryCreateDimensions(refsForTop, false);

            // Горизонтальные размеры
            var horizDimensionService = new TransViewRebarHorizDimensionService(Settings, doc, SheetInfo,
                                                                                ViewOfPylon, dimensionBaseService);
            horizDimensionService.TryCreateDimensions(refsForTop);
        } catch(Exception) { }

        // Пытаемся создать марки на виде
        try {
            var creator = new TransViewRebarMarkService(Settings, doc, SheetInfo, ViewOfPylon);
            creator.TryCreateBarMarks();
        } catch(Exception) { }
    }
}
