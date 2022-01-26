using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitLintelPlacement.Models {

    internal class RevitRepository {
        private readonly Application _application;
        private readonly UIApplication _uiApplication;

        private readonly Document _document;
        private readonly UIDocument _uiDocument;

        public RevitRepository(Application application, Document document) {
            _application = application;
            _uiApplication = new UIApplication(application);

            _document = document;
            _uiDocument = new UIDocument(document);
        }

        public IEnumerable<Element> GetElements(IEnumerable<ElementId> ids) {
            foreach(var id in ids)
                yield return _document.GetElement(id);
        }

        public IEnumerable<FamilyInstance> GetAllElementsInWall() {
            var categoryFilter = new ElementMulticategoryFilter(new List<BuiltInCategory> { BuiltInCategory.OST_Doors, BuiltInCategory.OST_Windows });

            return new FilteredElementCollector(_document)
                .WherePasses(categoryFilter)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>();
        }
    }
}
