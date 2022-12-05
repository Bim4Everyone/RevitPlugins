using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Extensions;

namespace RevitOpeningPlacement.Models.OpeningUnion {
    internal class OpeningsGroup {
        private List<Solid> _solids = new List<Solid>();

        public List<FamilyInstance> Elements { get; } = new List<FamilyInstance>();

        public List<FamilyInstance> Intersects(IList<FamilyInstance> elements) {
            List<FamilyInstance> intersections = new List<FamilyInstance>();
            var doc = elements.FirstOrDefault()?.Document;
            if(doc != null) {
                foreach(var solid in _solids) {
                    intersections.AddRange(
                        new FilteredElementCollector(doc, elements.Select(item => item.Id).ToArray())
                        .WherePasses(new ElementIntersectsSolidFilter(solid))
                        .OfType<FamilyInstance>());
                }
            }
            return intersections;
        }

        public void AddElement(FamilyInstance element) {
            Elements.Add(element);
            Solid unitedSolid = null;
            foreach(var solid in _solids) {
                try {
                    unitedSolid = BooleanOperationsUtils.ExecuteBooleanOperation(solid, element.GetSolid(), BooleanOperationsType.Union);
                    if(unitedSolid.Volume > 0) {
                        break;
                    }
                } catch {
                    continue;
                }
            }
            if(unitedSolid == null || unitedSolid.Volume <= 0) {
                _solids.Add(element.GetSolid());
            }
        }

        public void AddRangeElements(ICollection<FamilyInstance> familyInstances) {
            foreach(var fi in familyInstances) {
                AddElement(fi);
            }
        }
    }
}
