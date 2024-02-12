using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitOverridingGraphicsInViews.Models {
    internal class RevitRepository {
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;

        public List<View> AllViews => new FilteredElementCollector(Document)
               .OfClass(typeof(View))
               .WhereElementIsNotElementType()
               .OfType<View>()
               .Where(view => !view.IsTemplate)
               .ToList();
    }
}