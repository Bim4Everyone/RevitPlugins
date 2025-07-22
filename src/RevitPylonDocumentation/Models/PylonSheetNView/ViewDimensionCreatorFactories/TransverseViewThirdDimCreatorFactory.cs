using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreators;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreatorFactories;
internal class TransverseViewThirdDimCreatorFactory : IDimensionCreatorFactory {
    public ViewDimensionCreator Create(
    MainViewModel mvm,
    RevitRepository repository,
    PylonSheetInfo sheetInfo,
    PylonView view) {
        return new TransverseViewThirdDimCreator(mvm, repository, sheetInfo, view);
    }
}
