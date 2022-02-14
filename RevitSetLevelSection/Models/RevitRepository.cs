using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitSetLevelSection.Models {
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

        public Document Document => _document;
        public Application Application => _application;

        public ProjectInfo ProjectInfo => _document.ProjectInformation;

        public Element GetElements(ElementId elementId) {
            return _document.GetElement(elementId);
        }
        
        public IEnumerable<RevitLinkInstance> GetLinkInstances() {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(RevitLinkInstance))
                .OfType<RevitLinkInstance>()
                .ToList();
        }
        
        public IEnumerable<Element> GetElements(RevitParam revitParam) {
            var catFilter = new ElementMulticategoryFilter(GetCategories(revitParam));
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .WherePasses(catFilter)
                .ToList();
        }
        
        public IEnumerable<Element> GetElements(FamilyInstance massElement, RevitParam revitParam) {
            var bbFilter = new BoundingBoxIntersectsFilter(GetOutline(massElement));
            var catFilter = new ElementMulticategoryFilter(GetCategories(revitParam));

            var filter = new LogicalAndFilter(new ElementFilter[] { bbFilter, catFilter });
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .WherePasses(filter)
                .ToList();
        }

        public void UpdateElements(Parameter parameter, RevitParam revitParam, IEnumerable<Element> elements) {
            using(Transaction transaction = _document.StartTransaction("Установка уровня/секции")) {
                foreach(Element element in elements) {
                    Parameter elementParam = element.GetParam(revitParam);

                    if(parameter == null) {
                        elementParam.RemoveValue();
                    } else {
                        elementParam.Set(parameter);
                    }
                }
            }
        }

        public void UpdateElements(FamilyInstance massElement, RevitParam revitParam, IEnumerable<Element> elements) {
            Parameter massParameter = massElement.GetParam(revitParam);
            UpdateElements(massParameter, revitParam, elements);
        }

        private Outline GetOutline(FamilyInstance massElement) {
            var boundingBox = massElement.get_BoundingBox(_document.ActiveView);
            return new Outline(boundingBox.Min, boundingBox.Max);
        }

        private ElementId[] GetCategories(RevitParam revitParam) {
            return _document.GetParameterBindings()
                .Where(item => item.Binding.IsInstanceBinding())
                .Where(item => revitParam.IsRevitParam(_document, item.Definition))
                .SelectMany(item => item.Binding.GetCategories())
                .Select(item => item.Id)
                .ToArray();
        }
    }
}
