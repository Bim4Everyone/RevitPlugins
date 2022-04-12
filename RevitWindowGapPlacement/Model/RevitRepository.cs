using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitWindowGapPlacement.Model {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }

        public Application Application => UIApplication.Application;
        

        public Document Document => UIDocument.Document;
        public UIDocument UIDocument => UIApplication.ActiveUIDocument;
    }
}