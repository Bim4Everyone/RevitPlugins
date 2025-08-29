using System;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewFormDimensionServices;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewFormMarkServices;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class TransViewSecondAnnotCreator : ViewAnnotationCreator {
    public TransViewSecondAnnotCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                       PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewAnnotations() {
        // Пытаемся создать размеры на виде
        try {
            var dimensionService = new TransViewDimensionService(ViewModel, Repository, SheetInfo);
            dimensionService.TryCreateDimensions(ViewOfPylon.ViewElement, false, true);
        } catch(Exception) { }

        // Пытаемся создать марки на виде
        try {
            var markService = new TransViewMarkService(ViewModel, Repository, SheetInfo, ViewOfPylon);
            markService.TryCreateTransverseViewBarMarks();
        } catch(Exception) { }
    }
}
