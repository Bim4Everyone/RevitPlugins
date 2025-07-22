using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreators;
internal class TransverseRebarViewFirstDimCreator : ViewDimensionCreator {
    internal TransverseRebarViewFirstDimCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewDimensions() {

    }
}
