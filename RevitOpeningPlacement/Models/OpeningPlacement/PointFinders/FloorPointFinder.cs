
using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PointFinders {
    internal class FloorPointFinder : IPointFinder {
        private readonly MepCurveClash<CeilingAndFloor> _clash;

        public FloorPointFinder(MepCurveClash<CeilingAndFloor> clash) {
            _clash = clash;
        }

        public XYZ GetPoint() {
            var topFace = (PlanarFace) _clash.Element.GetTopFace();
            var topPlane = Plane.CreateByNormalAndOrigin(topFace.FaceNormal, topFace.Origin);
            var transformedTopPlane = _clash.ElementTransform.OfPlane(topPlane);
            var topPoint = _clash.Curve.GetLine().GetIntersectionWithPlane(transformedTopPlane);

            return topPoint;
        }
    }
}
