using RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreatorFactories;
internal interface IAnnotationCreatorFactory {
    internal ViewAnnotationCreator CreateAnnotationCreator(CreationSettings settings, RevitRepository repository,
                                                           PylonSheetInfo sheetInfo, PylonView view);
}
