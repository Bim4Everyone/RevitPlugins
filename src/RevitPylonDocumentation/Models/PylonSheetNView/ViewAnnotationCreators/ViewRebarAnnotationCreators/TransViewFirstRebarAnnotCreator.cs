using System;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices.ViewRebarMarkServices;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class TransViewFirstRebarAnnotCreator : ViewAnnotationCreator {
    internal TransViewFirstRebarAnnotCreator(MainViewModel mvm, RevitRepository repository, 
                                             PylonSheetInfo pylonSheetInfo, PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewAnnotations() {
        // Пытаемся создать размеры на виде
        try {
            var dimensionService = new TransViewRebarDimensionService(ViewModel, Repository, SheetInfo);
            dimensionService.TryCreateTransViewRebarDimensions(ViewOfPylon.ViewElement, false);
        } catch(Exception) { }

        // Пытаемся создать марки на виде
        try {
            var markService = new TransViewRebarMarkService(ViewModel, Repository, SheetInfo, ViewOfPylon);
            markService.TryCreateBarMarks();
            markService.TryCreatePlateMarks();
        } catch(Exception) { }
    }
}
