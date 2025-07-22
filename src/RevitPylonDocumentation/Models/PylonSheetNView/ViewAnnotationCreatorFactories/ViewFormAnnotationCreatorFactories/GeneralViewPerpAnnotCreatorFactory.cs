using RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreatorFactories;
internal class GeneralViewPerpAnnotCreatorFactory : IAnnotationCreatorFactory {
    public ViewAnnotationCreator CreateAnnotationCreator(MainViewModel mvm, RevitRepository repository,
                                                       PylonSheetInfo sheetInfo, PylonView view) {
        return new GeneralViewPerpAnnotCreator(mvm, repository, sheetInfo, view);
    }
}
