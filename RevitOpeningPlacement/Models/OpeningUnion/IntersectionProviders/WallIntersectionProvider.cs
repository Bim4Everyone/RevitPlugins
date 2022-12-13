using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningUnion.IntersectionProviders {
    internal class WallIntersectionProvider : IIntersectionProvider {
        public List<FamilyInstanceWrapper> GetIntersectedElements(OpeningsGroup group, IList<FamilyInstanceWrapper> elements) {
            var fi = group.Elements.First();
            var intersections = group.Intersects(elements.Select(item => item.Element).ToArray());
            return intersections.Where(item => HaveSameAngles(item, fi))
                .Select(item => new FamilyInstanceWrapper() { Element = item })
                .ToList();
        }

        private bool HaveSameAngles(FamilyInstance familyInstance1, FamilyInstance familyInstance2) {
            var transform1 = familyInstance1.GetTotalTransform();
            var transform2 = familyInstance2.GetTotalTransform();
            return transform1.BasisX.IsPapallel(transform2.BasisX)
                && transform1.BasisY.IsPapallel(transform2.BasisY)
                && transform1.BasisZ.IsPapallel(transform2.BasisZ);
        }
    }
}
