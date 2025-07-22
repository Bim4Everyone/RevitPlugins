using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
public abstract class ViewAnnotationCreator {

    internal ViewAnnotationCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, PylonView pylonView) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        ViewOfPylon = pylonView;
    }
    
    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }
    internal PylonView ViewOfPylon { get; set; }


    public abstract void TryCreateViewAnnotations();
}
