using System.Linq;
using System.Xml.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitCopyScheduleInstance.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public ViewSchedule GetSpecByName(string specName) => new FilteredElementCollector(Document)
            .OfClass(typeof(ViewSchedule))
            .OfType<ViewSchedule>()
            .FirstOrDefault(o => o.Name.Equals(specName));
    }
}
