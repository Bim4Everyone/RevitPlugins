using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.FilterGenerators;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.ClashDetection {
    internal class FilterProvider : IProvider {

        private readonly Filter _filter;

        public FilterProvider(Document doc, Filter filterElement, Transform transform) {
            Doc = doc;
            _filter = filterElement;
            MainTransform = transform;
        }

        public Document Doc { get; }
        public Transform MainTransform { get; }

        public List<Element> GetElements() {
            var categories = _filter.CategoryIds.Select(item => (BuiltInCategory)item).ToList();

            var elements = new FilteredElementCollector(Doc)
                .WherePasses(new ElementMulticategoryFilter(categories))
                .WherePasses(_filter.GetRevitFilter(Doc, new StraightRevitFilterGenerator()))
                .WhereElementIsNotElementType()
                .Where(item => item.get_Geometry(new Options()) != null)
                .ToList();
            return elements;
        }

        public List<Solid> GetSolids(Element element) {
            List<Solid> solids = new List<Solid>();
            var option = new Options() { ComputeReferences = true };
            foreach(GeometryObject geometryObject in element.get_Geometry(option)) {
                if(geometryObject is Solid solid) {
                    solids.Add(solid);
                } else {
                    var geometryInstance = geometryObject as GeometryInstance;
                    if(geometryInstance == null)
                        continue;

                    foreach(var s in geometryInstance.GetInstanceGeometry().OfType<Solid>()) {
                        solids.Add(s);
                    }
                }
            }

            return UniteSolids(solids);
        }

        private List<Solid> UniteSolids(List<Solid> solids) {

            if(solids.Count == 0) {
                return null;
            }
            Solid union = solids[0];
            solids.RemoveAt(0);

            List<Solid> unitedSolids = new List<Solid>();

            foreach(var s in solids) {
                try {
                    union = BooleanOperationsUtils.ExecuteBooleanOperation(union, s, BooleanOperationsType.Union);
                } catch {
                    unitedSolids.Add(union);
                    union = s;
                }
            }

            unitedSolids.Add(union);
            return unitedSolids;
        }
    }
}