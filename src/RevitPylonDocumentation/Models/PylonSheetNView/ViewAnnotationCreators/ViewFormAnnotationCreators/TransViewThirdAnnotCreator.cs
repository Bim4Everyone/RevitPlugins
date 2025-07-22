using System;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewFormDimensionServices;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkServices;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class TransViewThirdAnnotCreator : ViewAnnotationCreator {
    public TransViewThirdAnnotCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewAnnotations() {
        var dimensionService = new TransViewDimensionService(ViewModel, Repository, SheetInfo);
        dimensionService.TryCreateTransverseViewDimensions(ViewOfPylon.ViewElement, true);

        try {
            var creator = new TransverseViewMarkService(ViewModel, Repository, SheetInfo, ViewOfPylon);
            creator.CreateTransverseViewBarMarks();
        } catch(Exception) { }
    }
}
