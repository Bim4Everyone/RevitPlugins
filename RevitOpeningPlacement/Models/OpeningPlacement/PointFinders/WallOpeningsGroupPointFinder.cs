using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PointFinders {
    internal class WallOpeningsGroupPointFinder : RoundValueGetter, IPointFinder {
        private readonly OpeningsGroup _group;
        /// <summary>
        /// Округление высотной отметки отверстия в мм
        /// </summary>
        private const int _heightRound = 10;

        public WallOpeningsGroupPointFinder(OpeningsGroup group) {
            _group = group ?? throw new System.ArgumentNullException(nameof(group));
        }

        public XYZ GetPoint() {
            var transform = _group.Elements.First().GetFamilyInstance().GetTotalTransform();
            var bb = _group.Elements.Select(item => SolidUtils.CreateTransformed(item.GetSolid(), transform.Inverse))
                .Select(item => item.GetTransformedBoundingBox())
                .ToList()
                .CreateUnitedBoundingBox();
            var center = bb.Min + (bb.Max - bb.Min) / 2;
            var zRoundCoordinate = _group.IsCylinder ? RoundFeetToMillimeters(center.Z, _heightRound) : RoundFeetToMillimeters(bb.Min.Z, _heightRound);
            return _group.Elements.First().GetFamilyInstance().GetTotalTransform().OfPoint(new XYZ(center.X, bb.Min.Y, zRoundCoordinate));
        }
    }
}
