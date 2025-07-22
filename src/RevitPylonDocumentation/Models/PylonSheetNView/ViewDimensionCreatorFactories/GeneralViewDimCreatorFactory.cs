using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreators;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkCreators;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreatorFactories;
internal class GeneralViewDimCreatorFactory : IAnnotationCreatorFactory {
    public ViewDimensionCreator CreateDimensionCreator(MainViewModel mvm, RevitRepository repository, 
                                                       PylonSheetInfo sheetInfo, PylonView view) {
        return new GeneralViewDimCreator(mvm, repository, sheetInfo, view);
    }

    public ViewMarkCreator CreateMarkCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo sheetInfo, 
                                             PylonView view) {
        return new GeneralViewMarkCreator(mvm, repository, sheetInfo, view);
    }
}
