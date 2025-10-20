using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;
public abstract class ViewAnnotationCreator {
    internal ViewAnnotationCreator(CreationSettings settings, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                   PylonView pylonView) {
        Settings = settings;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        ViewOfPylon = pylonView;
    }
    
    internal CreationSettings Settings { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }
    internal PylonView ViewOfPylon { get; set; }

    public abstract void TryCreateViewAnnotations();
}
