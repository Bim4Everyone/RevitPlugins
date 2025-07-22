using RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreators;
using RevitPylonDocumentation.Models.PylonSheetNView.ViewMarkCreators;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreatorFactories;
internal class TransViewFirstRebarDimCreatorFactory : IAnnotationCreatorFactory {
    public ViewDimensionCreator CreateDimensionCreator(MainViewModel mvm, RevitRepository repository,
                                                       PylonSheetInfo sheetInfo, PylonView view) {
        return new TransViewFirstRebarDimCreator(mvm, repository, sheetInfo, view);
    }
    public ViewMarkCreator CreateMarkCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo sheetInfo,
                                         PylonView view) {
        return new TransViewFirstRebarMarkCreator(mvm, repository, sheetInfo, view);
    }
}
