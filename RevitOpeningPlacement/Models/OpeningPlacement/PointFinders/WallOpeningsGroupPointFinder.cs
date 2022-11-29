using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PointFinders {
    internal class WallOpeningsGroupPointFinder : IPointFinder {
        private readonly OpeningsGroup _group;

        public WallOpeningsGroupPointFinder(OpeningsGroup group) {
            _group = group;
        }

        public XYZ GetPoint() {
            var transform = _group.Elements.First().GetTotalTransform();
            var bb = _group.Elements.Select(item => SolidUtils.CreateTransformed(item.GetSolid(), transform.Inverse))
                .Select(item => item.GetTransformedBoundingBox())
                .ToList()
                .CreateUnitedBoundingBox();
            var center = bb.Min + (bb.Max - bb.Min) / 2;
            return _group.Elements.First().GetTotalTransform().OfPoint(new XYZ(center.X, bb.Min.Y, bb.Min.Z));
        }
    }
}
