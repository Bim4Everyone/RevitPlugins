
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PointFinders {
    internal class FloorPointFinder : IPointFinder {
        private readonly MepCurveClash<CeilingAndFloor> _clash;

        public FloorPointFinder(MepCurveClash<CeilingAndFloor> clash) {
            _clash = clash;
        }

        public XYZ GetPoint() {
            var topPoint = GetIntersectionPoint(_clash.Curve.GetLine(), (PlanarFace) _clash.Element.GetTopFace(), _clash.ElementTransform);
            var bottomPoint = GetIntersectionPoint(_clash.Curve.GetLine(), (PlanarFace) _clash.Element.GetBottomFace(), _clash.ElementTransform);

            var maxZ = new IntersectionGetter<CeilingAndFloor>(_clash).GetIntersection().GetOutline().MaximumPoint.Z;

            var point = bottomPoint + (topPoint - bottomPoint) / 2;
            return new XYZ(point.X, point.Y, maxZ);
        }

        private XYZ GetIntersectionPoint(Line line, PlanarFace face, Transform faceTransform) {
            var plane = Plane.CreateByNormalAndOrigin(face.FaceNormal, face.Origin);
            var transformedPlane = faceTransform.OfPlane(plane);
            return line.GetIntersectionWithPlane(transformedPlane);
        }
    }
}
