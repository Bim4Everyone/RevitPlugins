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

        public IEnumerable<DesignOption> GetDesignOptions() {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(DesignOption))
                .OfType<DesignOption>()
                .ToList();
        }

        public IEnumerable<FamilyInstance> GetMassElements(DesignOption designOption) {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Mass)
                .Where(item => item.DesignOption.Id == designOption.Id)
                .OfType<FamilyInstance>()
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


        public void UpdateElements(FamilyInstance massElement, RevitParam revitParam, IEnumerable<Element> elements) {
            var massParameter = massElement.GetParam(revitParam);
            using(var transaction = _document.StartTransaction("Установка уровня/секции")) {
                foreach(var element in elements) {
                    var elementParam = element.GetParam(revitParam);
                    elementParam.Set(massParameter);
                }
            }
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
