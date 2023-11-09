using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders {
    internal class WallAngleFinder : IAngleFinder {
        private readonly Wall _wall;
        private readonly Transform _transform;

        public WallAngleFinder(Wall wall, Transform transform) {
            _wall = wall;
            _transform = transform;
        }

        public Rotates GetAngle() {
            var orientation = _transform.OfVector(_wall.Orientation);
            var angle = orientation.AngleTo(XYZ.BasisY);
            return (orientation.X <= 0 && orientation.Y <= 0) || (orientation.X <= 0 && orientation.Y >= 0) ? new Rotates(0, 0, angle) : new Rotates(0, 0, -angle);
        }
    }
}
