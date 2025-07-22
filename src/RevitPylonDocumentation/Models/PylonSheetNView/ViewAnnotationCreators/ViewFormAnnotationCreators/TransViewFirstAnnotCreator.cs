using System;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewFormDimensionServices;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class TransViewFirstAnnotCreator : ViewAnnotationCreator {
    internal TransViewFirstAnnotCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewAnnotations() {
        // Пытаемся создать размеры на виде
        try {
            var dimensionService = new TransViewDimensionService(ViewModel, Repository, SheetInfo);
            dimensionService.TryCreateTransverseViewDimensions(ViewOfPylon.ViewElement, false);
        } catch(Exception) { }

        // Пытаемся создать марки на виде
        try {
            var markService = new TransverseViewMarkService(ViewModel, Repository, SheetInfo, ViewOfPylon);
            markService.CreateTransverseViewBarMarks();
        } catch(Exception) { }
    }
}
