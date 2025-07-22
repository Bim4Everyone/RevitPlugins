using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreators;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkCreators;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreatorFactories;
internal interface IAnnotationCreatorFactory {
    internal ViewDimensionCreator CreateDimensionCreator(MainViewModel mvm, RevitRepository repository,
                                                         PylonSheetInfo sheetInfo, PylonView view);

    internal ViewMarkCreator CreateMarkCreator(MainViewModel mvm, RevitRepository repository,
                                               PylonSheetInfo sheetInfo, PylonView view);
}
