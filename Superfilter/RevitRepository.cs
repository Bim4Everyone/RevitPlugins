using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Superfilter {
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

        public IList<Element> GetAllElements() {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .ToElements();
        }

        public IList<Element> GetElements() {
            return GetElements(_document.ActiveView);
        }

        public IList<Element> GetElements(View view) {
            return new FilteredElementCollector(_document, view.Id)
                .WhereElementIsNotElementType()
                .ToElements();
        }

        public IList<Element> GetSelectedElements() {
            return _uiDocument.Selection.GetElementIds()
                .Select(item => _document.GetElement(item))
                .ToList();
        }

        public IList<Category> GetCategories() {
            return Enum.GetValues(typeof(BuiltInCategory))
                .OfType<BuiltInCategory>()
                .Select(item => Category.GetCategory(_document, item))
                .ToList();
        }

        public IList<Category> GetCategories(IEnumerable<Element> elements) {
            return elements.Select(item => item.Category).Distinct(new CategoryComparer()).ToList();
        }

        public void SetSelectedElements(IEnumerable<Element> elements) {
            _uiDocument.Selection.SetElementIds(elements.Select(item => item.Id).ToList());
        }
    }

    internal class CategoryComparer : IEqualityComparer<Category> {
        public bool Equals(Category x, Category y) {
            if(x == null && y == null) {
                return true;
            }

            if(x == null || y == null) {
                return false;
            }

            return x.Name.Equals(y.Name);
        }

        public int GetHashCode(Category obj) {
            return obj?.Name.GetHashCode() ?? 0;
        }
    }
}
