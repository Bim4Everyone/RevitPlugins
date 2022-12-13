
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningUnion.IntersectionProviders {
    internal class FloorIntersectionProvider : IIntersectionProvider {
        public List<FamilyInstanceWrapper> GetIntersectedElements(OpeningsGroup group, IList<FamilyInstanceWrapper> elements) {
            return group.Intersects(elements.Select(item => item.Element).ToArray())
                .Select(item => new FamilyInstanceWrapper() { Element = item })
                .ToList();
        }
    }
}
