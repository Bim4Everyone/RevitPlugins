using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitClashDetective.Models.Extensions {
    internal static class ElementExtensions {
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
            return geometryInstance.GetInstanceGeometry().OfType<Solid>();
        }

        public static Solid GetSolid(this Element element) {
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

            unions.Add(union);
            return unions;
        }
    }
}
