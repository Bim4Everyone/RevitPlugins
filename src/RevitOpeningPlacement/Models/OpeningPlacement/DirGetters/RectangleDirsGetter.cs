using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.DirGetters {
    internal class RectangleDirsGetter : IDirectionsGetter {
        private readonly MepCurveClash<Wall> _clash;

        public RectangleDirsGetter(MepCurveClash<Wall> clash) {
            _clash = clash;
        }

        public IEnumerable<XYZ> GetDirectionsOnPlane(Plane plane) {
            //получение векторов для смещения в плоскости коннектора инженерной системы
            var height = _clash.Element1.GetHeight();
            var width = _clash.Element1.GetWidth();
            var coordinateSystem = _clash.Element1.GetConnectorCoordinateSystem();
            var dirX = coordinateSystem.BasisX;
            var dirY = coordinateSystem.BasisY;

            //умножение единичных векторов на длину
            var verticalDirs = new[] { dirY, -dirY }.Select(item => item * height);
            var horizontalDirs = new[] { dirX, -dirX }.Select(item => item * width);

            // сложение векторов для получение направлений всех диагоналей и проекция полученных векторов на плоскость
            return verticalDirs.SelectMany(item => horizontalDirs.Select(hitem => _clash.Element2Transform.OfVector(hitem + item)))
                               .Select(item => plane.ProjectVector(item).Normalize());
        }
    }
}
