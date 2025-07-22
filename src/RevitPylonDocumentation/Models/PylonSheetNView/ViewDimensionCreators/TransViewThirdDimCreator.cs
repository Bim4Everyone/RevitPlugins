using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreators;
internal class TransViewThirdDimCreator : ViewDimensionCreator {
    public TransViewThirdDimCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewDimensions() {
        var creator = new TransverseViewDimensionsCreator(ViewModel, Repository, SheetInfo);
        creator.TryCreateTransverseViewDimensions(ViewOfPylon.ViewElement, true);
    }
}
