
using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Projectors {
    internal class NormalWallPlaneProjection : IProjector {
        private Plane _plane;
        private readonly Transform _transform;

        public NormalWallPlaneProjection(Wall wall, Transform transform) {
            _transform = transform;
            InitializePlane(wall);
        }

        private void InitializePlane(Wall wall) {
            var line = wall.GetСentralLine()
                           .GetTransformedLine(_transform);
            _plane = Plane.CreateByNormalAndOrigin(line.Direction, line.Origin);
        }

        public XYZ ProjectPoint(XYZ point) {
            return _plane.ProjectPoint(point);
        }

        public double GetAngleOnPlaneToAxis(XYZ xyz) {
            return XYZ.BasisZ.AngleOnPlaneTo(xyz, _plane.Normal);
        }

        public XYZ ProjectVector(XYZ vector) {
            return _plane.ProjectVector(vector);
        }
    }
}
