using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitAreaBoundaries.Models;

internal class RevitRepository {

    public RevitRepository(UIApplication uiApplication) {
        UiApplication = uiApplication;
    }

    private UIApplication UiApplication { get; }
    public UIDocument ActiveUiDocument => UiApplication.ActiveUIDocument;
    public Application Application => UiApplication.Application;
    public Document Document => ActiveUiDocument.Document;
    
}
