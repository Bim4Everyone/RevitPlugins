using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreators;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreatorFactories;
internal class TransverseViewSecondDimCreatorFactory : IDimensionCreatorFactory {
    public ViewDimensionCreator Create(
        MainViewModel mvm,
        RevitRepository repository,
        PylonSheetInfo sheetInfo,
        PylonView view) {
        return new TransverseViewSecondDimCreator(mvm, repository, sheetInfo, view);
    }
}
