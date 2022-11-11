
using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders {
    internal class FloorAngleFinder : IAngleFinder {
        private readonly MEPCurve _curve;

        public FloorAngleFinder(MEPCurve curve) {
            _curve = curve;
        }

        public Rotates GetAngle() {
            var transform = _curve.GetConnectorCoordinateSystem();
            return new Rotates(0, 0, new XYZ(1, 0, 0).AngleOnPlaneTo(transform.BasisX, new XYZ(0, 0, 1)));
            //семейство нельзя повернуть вокруг осей x и y
        }
    }
}
