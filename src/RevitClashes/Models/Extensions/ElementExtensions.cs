using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitClashDetective.Models.Extensions {
    internal static class ElementExtensions {
        public static bool IsFromDocument(this Element element, Document doc) {
            return element.Document.Equals(doc);
        }

        public static List<Solid> GetSolids(this Element element) {
            var options = new Options() { ComputeReferences = true, IncludeNonVisibleObjects = false, DetailLevel = ViewDetailLevel.Fine };
            return element.get_Geometry(options)
                .SelectMany(item => item.GetSolids())
                .GetUnitedSolids()
                .ToList();
        }

        public static IEnumerable<Solid> GetSolids(this GeometryObject geometryObject) {
            if(geometryObject is Solid solid) {
                yield return solid;
            } else if(geometryObject is GeometryInstance geometryInstance) {
                foreach(var instance in geometryInstance.GetSolids()) {
                    yield return instance;
                }
            }
        }

        public static IEnumerable<Solid> GetSolids(this GeometryInstance geometryInstance) {
            return geometryInstance.GetInstanceGeometry().SelectMany(item => item.GetSolids()).Where(item => item.Volume > 0);
        }

        public static Solid GetSolid(this Element element) {
            var solids = element.GetSolids();

            return UniteSolids(solids);
        }

        public static Solid UniteSolids(List<Solid> solids) {
            return GetUnitedSolids(solids).OrderByDescending(s => s.Volume).FirstOrDefault();
        }

        public static IEnumerable<Solid> GetUnitedSolids(this IEnumerable<Solid> solids) {
            Solid union = solids.FirstOrDefault();
            var unions = new List<Solid>();
            foreach(var s in solids.Skip(1)) {
                try {
                    union = BooleanOperationsUtils.ExecuteBooleanOperation(union, s, BooleanOperationsType.Union);
                } catch {
                    unions.Add(union);
                    union = s;
                }
            }

            if(!(union is null)) {
                unions.Add(union);
            }
            return unions;
        }
    }
}
