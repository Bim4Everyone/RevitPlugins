using System;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewRebarMarkServices;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class TransViewSecondRebarAnnotCreator : ViewAnnotationCreator {
    public TransViewSecondRebarAnnotCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                            PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewAnnotations() {
        // Пытаемся создать размеры на виде
        try {
            var dimensionService = new TransViewRebarDimensionService(ViewModel, Repository, SheetInfo);
            dimensionService.TryCreateTransViewRebarDimensions(ViewOfPylon.ViewElement, true);
        } catch(Exception) { } 

        // Пытаемся создать марки на виде
        try {
            var creator = new TransViewRebarMarkService(ViewModel, Repository, SheetInfo, ViewOfPylon);
            creator.TryCreateBarMarks();
        } catch(Exception) { }
    }
}
