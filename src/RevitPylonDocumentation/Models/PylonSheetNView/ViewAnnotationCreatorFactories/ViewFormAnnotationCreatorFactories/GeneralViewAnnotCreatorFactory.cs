using RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreatorFactories;
internal class GeneralViewAnnotCreatorFactory : IAnnotationCreatorFactory {
    public ViewAnnotationCreator CreateAnnotationCreator(MainViewModel mvm, RevitRepository repository, 
                                                       PylonSheetInfo sheetInfo, PylonView view) {
        return new GeneralViewAnnotCreator(mvm, repository, sheetInfo, view);
    }
}
