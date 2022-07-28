using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public double GetAngle() {
            var orientation = _wall.Orientation;
            var angle = _transform.OfVector(orientation).AngleTo(XYZ.BasisY);
            return (orientation.X <= 0 && orientation.Y <= 0) || (orientation.X <= 0 && orientation.Y >= 0) ? angle : -angle;
        }
    }
}
