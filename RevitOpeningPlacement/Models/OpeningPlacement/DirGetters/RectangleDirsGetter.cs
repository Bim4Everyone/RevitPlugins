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
        private readonly MepCurveClash<Wall> _clash;

        public RectangleDirsGetter(MepCurveClash<Wall> clash) {
            _clash = clash;
        }

        public IEnumerable<XYZ> GetDirectionsOnPlane(Plane plane) {
            //получение векторов для смещения в плоскости коннектора инженерной системы
            var height = _clash.Curve.GetHeight();
            var width = _clash.Curve.GetWidth();
            var coordinateSystem = _clash.Curve.GetConnectorCoordinateSystem();
            var dirX = coordinateSystem.BasisX;
            var dirY = coordinateSystem.BasisY;

            //умножение единичных векторов на длину
            var verticalDirs = new[] { dirY, -dirY }.Select(item => item * height);
            var horizontalDirs = new[] { dirX, -dirX }.Select(item => item * width);

            // сложение векторов для получение направлений всех диагоналей и проекция полученных векторов на плоскость
            return verticalDirs.SelectMany(item => horizontalDirs.Select(hitem => _clash.ElementTransform.OfVector(hitem + item)))
                               .Select(item => plane.ProjectVector(item).Normalize());
        }
    }
}
