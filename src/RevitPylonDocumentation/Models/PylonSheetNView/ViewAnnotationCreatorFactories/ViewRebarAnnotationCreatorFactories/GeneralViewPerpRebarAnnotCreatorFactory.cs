using RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreatorFactories;
internal class GeneralViewPerpRebarAnnotCreatorFactory : IAnnotationCreatorFactory {
    public ViewAnnotationCreator CreateAnnotationCreator(MainViewModel mvm, RevitRepository repository,
                                                         PylonSheetInfo sheetInfo, PylonView view) {
        return new GeneralViewRebarPerpAnnotCreator(mvm, repository, sheetInfo, view);
    }
}
