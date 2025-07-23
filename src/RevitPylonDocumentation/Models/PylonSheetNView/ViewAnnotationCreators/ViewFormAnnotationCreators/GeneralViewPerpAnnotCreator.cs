using System;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
internal class GeneralViewPerpAnnotCreator : ViewAnnotationCreator {
    internal GeneralViewPerpAnnotCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                         PylonView pylonView) 
        : base(mvm, repository, pylonSheetInfo, pylonView) {
    }

    public override void TryCreateViewAnnotations() {
        // Пытаемся создать размеры на виде
        try {
        } catch(Exception) { }

        // Пытаемся создать марки на виде
        try {
        } catch(Exception) { }
    }
}
