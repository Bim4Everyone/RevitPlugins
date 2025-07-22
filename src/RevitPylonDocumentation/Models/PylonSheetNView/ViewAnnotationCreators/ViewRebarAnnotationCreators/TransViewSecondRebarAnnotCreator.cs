using System;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class TransViewSecondRebarAnnotCreator : ViewAnnotationCreator {
    public TransViewSecondRebarAnnotCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewAnnotations() {
        var dimensionService = new TransViewRebarDimensionService(ViewModel, Repository, SheetInfo);
        dimensionService.TryCreateTransViewRebarDimensions(ViewOfPylon.ViewElement, true);

        try {
            var creator = new TransverseViewRebarMarkService(ViewModel, Repository, SheetInfo, ViewOfPylon);
            creator.CreateTransverseRebarViewBarMarks();
            creator.CreateTransverseRebarViewPlateMarks();
        } catch(Exception) { }
    }
}
