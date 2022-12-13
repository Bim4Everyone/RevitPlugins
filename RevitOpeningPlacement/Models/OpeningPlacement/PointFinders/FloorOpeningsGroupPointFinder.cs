using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PointFinders {
    internal class FloorOpeningsGroupPointFinder : IPointFinder {
        private readonly OpeningsGroup _group;

        public FloorOpeningsGroupPointFinder(OpeningsGroup group) {
            _group = group;
        }

        public XYZ GetPoint() {
            var bb = _group.Elements.Select(item => item.GetSolid().GetTransformedBoundingBox())
                .ToList()
                .CreateUnitedBoundingBox();
            var center = bb.Min + (bb.Max - bb.Min) / 2;
            return new XYZ(center.X, center.Y, bb.Max.Z);
        }
    }
}
