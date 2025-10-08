using System;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewRebarMarkServices;
using RevitPylonDocumentation.Models.Services;
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
            var dimensionBaseService = new DimensionBaseService(ViewOfPylon.ViewElement, ViewModel.ParamValService);
            var dimensionService = new TransViewRebarDimensionService(ViewModel, Repository, SheetInfo, ViewOfPylon,
                                                                      dimensionBaseService);
            dimensionService.TryCreateTransViewRebarDimensions(false, true);
        } catch(Exception) { } 

        // Пытаемся создать марки на виде
        try {
            var creator = new TransViewRebarMarkService(ViewModel, Repository, SheetInfo, ViewOfPylon);
            creator.TryCreateBarMarks();
        } catch(Exception) { }
    }
}
