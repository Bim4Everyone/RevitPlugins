using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitClashDetective.Models.Extensions {
    internal static class ElementExtensions {
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
        private static Solid UniteSolids(List<Solid> solids) {

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
            return unitedSolids.FirstOrDefault(item => Math.Abs(item.Volume - unitedSolids.Max(s => s.Volume)) < 0.0001);
        }
    }
}
