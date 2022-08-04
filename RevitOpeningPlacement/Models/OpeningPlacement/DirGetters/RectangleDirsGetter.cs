using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.DirGetters {
    internal class RectangleDirsGetter : IDirectionsGetter {
        private readonly MepCurveWallClash _clash;
        private readonly Plane _plane;

        public RectangleDirsGetter(MepCurveWallClash clash, Plane plane) {
            _clash = clash;
            _plane = plane;
        }

        public IEnumerable<XYZ> GetDirections() {
            var height = _clash.Curve.GetHeight();
            var width = _clash.Curve.GetWidth();
            var coordinateSystem = _clash.Curve.GetConnectorCoordinateSystem();
            var dirX = coordinateSystem.BasisX;
            var dirY = coordinateSystem.BasisY;

            var verticalDirs = new[] { dirY, -dirY }.Select(item => item * height);
            var horizontalDirs = new[] { dirX, -dirX }.Select(item => item * width);

            return verticalDirs.SelectMany(item => horizontalDirs.Select(hitem => _clash.WallTransform.OfVector(hitem + item)))
                               .Select(item => _plane.ProjectVector(item).Normalize());
        }
    }
}
