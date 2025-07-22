using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreators;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreatorFactories;
internal class TransverseViewFirstDimCreatorFactory : IDimensionCreatorFactory {
    public ViewDimensionCreator Create(
        MainViewModel mvm,
        RevitRepository repository,
        PylonSheetInfo sheetInfo,
        PylonView view) {
        return new TransverseViewFirstDimCreator(mvm, repository, sheetInfo, view);
    }
}
