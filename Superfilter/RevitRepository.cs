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
        private readonly Document _document;
        private readonly UIDocument _uiDocument;

        private readonly Application _application;
        private readonly UIApplication _uiApplication;

        public RevitRepository(Application application, Document document) {
            _application = application;
            _uiApplication = new UIApplication(application);

            _document = document;
            _uiDocument = new UIDocument(document);
        }

        public IList<Element> GetElements() {
            return new FilteredElementCollector(_document)
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
    }
}
