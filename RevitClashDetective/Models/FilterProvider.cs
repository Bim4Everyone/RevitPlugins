using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.Models {
    internal class FilterProvider : IProvider {

        private readonly Document _doc;
        private readonly Filter _filter;
        private Transform _transform;

        public FilterProvider(Document doc, Filter filterElement, Transform transform) {
            _doc = doc;
            _filter = filterElement;
            _transform = transform;
        }


        public List<Element> GetElements() {
            var categories = _filter.CategoryIds.Select(item => (BuiltInCategory)item).ToList();

            var elements = new FilteredElementCollector(_doc)
                .WherePasses(new ElementMulticategoryFilter(categories))
                .WherePasses(_filter.GetRevitFilter(_doc))
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
                    union = SolidUtils.CreateTransformed(union, _transform);
                    unitedSolids.Add(union);
                    union = s;
                }
            }
            union = SolidUtils.CreateTransformed(union, _transform);
            unitedSolids.Add(union);
            return unitedSolids;
        }

        public Outline GetOutline(Solid solid) {
            var bb = solid.GetBoundingBox();
            XYZ pt0 = new XYZ(bb.Min.X, bb.Min.Y, bb.Min.Z);
            XYZ pt1 = new XYZ(bb.Max.X, bb.Min.Y, bb.Min.Z);
            XYZ pt2 = new XYZ(bb.Max.X, bb.Max.Y, bb.Min.Z);
            XYZ pt3 = new XYZ(bb.Min.X, bb.Max.Y, bb.Min.Z);

            var transform = bb.Transform;

            var tpt0 = transform.OfPoint(pt0);
            var tpt1 = transform.OfPoint(pt1);
            var tpt2 = transform.OfPoint(pt2);
            var tpt3 = transform.OfPoint(pt3);

            var tMax = transform.OfPoint(bb.Max);
            var points = new List<XYZ> { tpt0, tpt1, tpt2, tpt3 };

            var min = new XYZ(points.Min(p => p.X), points.Min(p => p.Y), points.Min(p => p.Z));
            var max = new XYZ(points.Max(p => p.X), points.Max(p => p.Y), tMax.Z);
            return new Outline(min, max);
        }
    }
}