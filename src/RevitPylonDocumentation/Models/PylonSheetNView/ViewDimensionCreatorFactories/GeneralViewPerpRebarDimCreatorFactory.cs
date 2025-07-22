using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreators;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkCreators;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreatorFactories;
internal class GeneralViewPerpRebarDimCreatorFactory : IAnnotationCreatorFactory {
    public ViewDimensionCreator CreateDimensionCreator(MainViewModel mvm, RevitRepository repository,
                                                       PylonSheetInfo sheetInfo, PylonView view) {
        return new GeneralViewPerpRebarDimCreator(mvm, repository, sheetInfo, view);
    }
    public ViewMarkCreator CreateMarkCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo sheetInfo,
                                             PylonView view) {
        return new GeneralViewPerpRebarMarkCreator(mvm, repository, sheetInfo, view);
    }
}
