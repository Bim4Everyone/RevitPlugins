using RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreatorFactories;
internal interface IAnnotationCreatorFactory {
    internal ViewAnnotationCreator CreateAnnotationCreator(MainViewModel mvm, RevitRepository repository,
                                                         PylonSheetInfo sheetInfo, PylonView view);
}
