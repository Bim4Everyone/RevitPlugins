using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

namespace RevitClashDetective.Models {
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

        public Element GetElement(ElementId id) {
            return _document.GetElement(id);
        }

        public FilteredElementCollector GetClashCollector() {
            var view = GetNavisworksView();
            if(view != null) {
                return new FilteredElementCollector(_document, view.Id);
            }
            return null;
        }

        public View GetNavisworksView() {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(View))
                .Cast<View>()
                .FirstOrDefault(item => item.Name == "Navisworks");
        }

        public Document Doc => _document;

        public LanguageType GetLanguage() {
            return _application.Language;
        }
        public List<ParameterFilterElement> GetFilters() {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(ParameterFilterElement))
                .Cast<ParameterFilterElement>()
                .Where(item => item.Name.StartsWith("BIM"))
                .ToList();
        }

        public List<RevitLinkInstance> GetRevitLinkInstances() {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .ToList();
        }


        public IEnumerable<ClashModel> GetClashes(ClashDetector detector) {
            return detector.FindClashes(_document);
        }

        public void SelectElements(IEnumerable<Element> elements) {
            var selection = _uiApplication.ActiveUIDocument.Selection;
            selection.SetElementIds(elements.Select(e => e.Id).ToList());
        }

        public List<Category> GetCategories() {
            return ParameterFilterUtilities.GetAllFilterableCategories()
                .Select(item => Category.GetCategory(_document, item))
                .OfType<Category>()
                .ToList();
        }

        public List<ElementId> GetParameters(IEnumerable<Category> categories) {
            return ParameterFilterUtilities
                .GetFilterableParametersInCommon(_document,
                    categories.Select(c => c.Id).ToList())
                .ToList();

            //TODO: для связанных документов
        }
    }
}
