using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;

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


        public ViewSheet CreateViewSheet(FamilySymbol familySymbol) {
            return ViewSheet.Create(Document, familySymbol.Id);
        }

        public List<string> GetAlbumsBlueprints() {
            return GetViewSheets()
                .Select(item => (string) item.GetParamValueOrDefault("ADSK_Комплект чертежей"))
                .OrderBy(item => item)
                .Distinct()
                .ToList();
        }

        public List<ViewSheet> GetViewSheets() {
            return new FilteredElementCollector(Document)
                .OfClass(typeof(ViewSheet))
                .OfType<ViewSheet>()
                .ToList();
        }

        public List<FamilySymbol> GetTitleBlocks() {
            var category = Category.GetCategory(Document, BuiltInCategory.OST_TitleBlocks);
            return new FilteredElementCollector(Document)
                .OfClass(typeof(FamilySymbol))
                .OfType<FamilySymbol>()
                .Where(item => item.Category.Id == category.Id)
                .ToList();
        }
    }
}
