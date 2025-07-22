using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreators;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreatorFactories;
internal class GeneralRebarViewPerpDimCreatorFactory : IDimensionCreatorFactory {
    public ViewDimensionCreator Create(
        MainViewModel mvm,
        RevitRepository repository,
        PylonSheetInfo sheetInfo,
        PylonView view) {
        return new GeneralRebarViewPerpDimCreator(mvm, repository, sheetInfo, view);
    }
}
