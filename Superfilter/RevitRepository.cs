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
            return new FilteredElementCollector(_document, _document.ActiveView.Id)
                .WhereElementIsNotElementType()
                .ToElements();
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
            return elements.Select(item => item.Category).Distinct().ToList();
        }
    }
}
