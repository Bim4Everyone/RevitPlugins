using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

using dosymep.Revit;

using RevitClashDetective.Handlers;

namespace RevitClashDetective.Models {
    internal class RevitRepository {
        private readonly Application _application;
        private readonly UIApplication _uiApplication;

        private readonly Document _document;
        private readonly UIDocument _uiDocument;
        private readonly RevitEventHandler _revitEventHandler;

        public RevitRepository(Application application, Document document) {
            _application = application;
            _uiApplication = new UIApplication(application);

            _document = document;
            _uiDocument = new UIDocument(document);

            _revitEventHandler = new RevitEventHandler();
        }

        public Element GetElement(ElementId id) {
            return _document.GetElement(id);
        }

        public Element GetElement(Document doc, ElementId id) {
            return doc.GetElement(id);
        }

        public List<Collector> GetCollectors() {
            return GetDocuments()
                .Select(item => new Collector(item))
                .ToList();
        }

        public View GetNavisworksView(Document doc) {
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
                .Where(item=>item.GetLinkDocument()!=null)
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

        public Category GetCategory(BuiltInCategory builtInCategory) {
            return Category.GetCategory(_document, builtInCategory);
        }

        public List<ElementId> GetParameters(Document doc, IEnumerable<Category> categories) {
            return ParameterFilterUtilities
                .GetFilterableParametersInCommon(doc,
                    categories.Select(c => c.Id).ToList())
                .ToList();
        }

        public IEnumerable<Document> GetDocuments() {
            var linkedDocuments = new FilteredElementCollector(_document)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .Select(item => item.GetLinkDocument())
                .Where(item=>item!=null)
                .ToList();
            linkedDocuments.Add(_document);
            return linkedDocuments;
        }

        public void CreateFilter(IEnumerable<ElementId> categories, ElementFilter filter, string name) {
            using(Transaction t = _document.StartTransaction("Создание фильтра")) {
                ParameterFilterElement pfe = ParameterFilterElement.Create(_document, name, categories.ToList());
                pfe.SetElementFilter(filter);
                t.Commit();
            }
            
        }

        public async Task SelectAndShowElement(ElementId id) {
            _revitEventHandler.TransactAction = () => {
                _uiDocument.Selection.SetElementIds(new[] { id });
                var commandId = RevitCommandId.LookupCommandId("ID_VIEW_APPLY_SELECTION_BOX");
                if(!(commandId is null) && _uiDocument.Application.CanPostCommand(commandId)) {
                    _uiApplication.PostCommand(commandId);
                }
            };
            await _revitEventHandler.Raise();
        }
    }
}
