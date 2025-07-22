using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreators;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreatorFactories;
internal class GeneralViewDimCreatorFactory : IDimensionCreatorFactory {
    public ViewDimensionCreator Create(
        MainViewModel mvm,
        RevitRepository repository,
        PylonSheetInfo sheetInfo,
        PylonView view) {
        return new GeneralViewDimCreator(mvm, repository, sheetInfo, view);
    }
}
