using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitCreateViewSheet.Models {
    public class RevitRepository {
        private readonly UIApplication _uiApplication;

        public RevitRepository(UIApplication uiApplication) {
            _uiApplication = uiApplication;
        }

        public Application Application {
            get => _uiApplication.Application;
        }

        public Document Document {
            get => UIDocument.Document;
        }

        public UIDocument UIDocument {
            get => _uiApplication.ActiveUIDocument;
        }


        public List<ViewSheet> GetViewSheets() {
            return new FilteredElementCollector(Document)
                .OfClass(typeof(ViewSheet))
                .OfType<ViewSheet>()
                .ToList();
        }

        public List<FamilyInstance> GetTitleBlocks() {
            return new FilteredElementCollector(Document)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .OfType<FamilyInstance>()
                .ToList();
        }
    }
}
