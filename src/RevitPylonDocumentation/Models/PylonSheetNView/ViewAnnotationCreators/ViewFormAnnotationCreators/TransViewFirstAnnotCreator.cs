using System;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewFormDimensionServices;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewFormMarkServices;
using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class TransViewFirstAnnotCreator : ViewAnnotationCreator {
    internal TransViewFirstAnnotCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                        PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewAnnotations() {
        // Пытаемся создать размеры на виде
        try {
            var dimensionBaseService = new DimensionBaseService(ViewOfPylon.ViewElement, ViewModel.ParamValService);
            var dimensionService = new TransViewDimensionService(ViewModel, Repository, SheetInfo, ViewOfPylon, 
                                                                 dimensionBaseService);
            dimensionService.TryCreateDimensions(false, false);
        } catch(Exception) { }

        // Пытаемся создать марки на виде
        try {
            var markService = new TransViewMarkService(ViewModel, Repository, SheetInfo, ViewOfPylon);
            markService.TryCreateTransverseViewBarMarks();
        } catch(Exception) { }
    }
}
