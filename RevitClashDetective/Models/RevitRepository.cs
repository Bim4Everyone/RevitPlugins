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
        public Document Doc => _document;

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

        public List<ParameterModel> GetParameters(IEnumerable<Category> categories) {
            var documents = new FilteredElementCollector(_document)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .Select(item => item.GetLinkDocument())
                .ToList();
            documents.Add(_document);
            List<ParameterModel> parameters = new List<ParameterModel>();
            var categoryIds = categories.Select(c => c.Id).ToList();
            foreach(var doc in documents) {
                var parameterIds = ParameterFilterUtilities.GetFilterableParametersInCommon(doc, categoryIds);
                parameters.AddRange(parameterIds
                    .Where(item => item.IntegerValue > 0)
                    .Select(item => doc.GetElement(item))
                    .OfType<ParameterElement>()
                    .Select(item => new ParameterModel() { Name = item.GetDefinition().Name }));
                parameters.AddRange(parameterIds
                    .Where(item => item.IntegerValue < 0)
                    .Select(item => new ParameterModel() { Name = LabelUtils.GetLabelFor((BuiltInParameter) item.IntegerValue) }));
            }
            return parameters;
        }
    }
}
