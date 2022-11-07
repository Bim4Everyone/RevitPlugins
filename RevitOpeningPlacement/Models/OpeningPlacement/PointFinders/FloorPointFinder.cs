
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
            var topPoint = GetIntersectionPoint(_clash.Curve.GetLine(), (PlanarFace) _clash.Element.GetTopFace(), _clash.ElementTransform);
            var bottomPoint = GetIntersectionPoint(_clash.Curve.GetLine(), (PlanarFace) _clash.Element.GetBottomFace(), _clash.ElementTransform);
            
            var topFace = (PlanarFace) _clash.Element.GetTopFace();
            var topPlane = Plane.CreateByNormalAndOrigin(topFace.FaceNormal, topFace.Origin);
            var transformedTopPlane = _clash.ElementTransform.OfPlane(topPlane);

            return transformedTopPlane.ProjectPoint(bottomPoint + (topPoint - bottomPoint) / 2);
        }

        private XYZ GetIntersectionPoint(Line line, PlanarFace face, Transform faceTransform) {
            var plane = Plane.CreateByNormalAndOrigin(face.FaceNormal, face.Origin);
            var transformedPlane = faceTransform.OfPlane(plane);
            return line.GetIntersectionWithPlane(transformedPlane);
        }
    }
}
