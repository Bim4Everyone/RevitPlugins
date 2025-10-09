using System;

using RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreatorFactories;
internal class AnnotationCreatorFactory<T> : IAnnotationCreatorFactory where T : ViewAnnotationCreator {
    public ViewAnnotationCreator CreateAnnotationCreator(MainViewModel mvm, RevitRepository repository, 
                                                         PylonSheetInfo sheetInfo, PylonView view) {
        return Activator.CreateInstance(typeof(T), mvm, repository, sheetInfo, view) as ViewAnnotationCreator;
    }
}
