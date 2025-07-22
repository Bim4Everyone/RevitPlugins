using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreators;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreatorFactories;
internal interface IDimensionCreatorFactory {
    internal ViewDimensionCreator Create(
        MainViewModel mvm,
        RevitRepository repository,
        PylonSheetInfo sheetInfo,
        PylonView view
    );
}
