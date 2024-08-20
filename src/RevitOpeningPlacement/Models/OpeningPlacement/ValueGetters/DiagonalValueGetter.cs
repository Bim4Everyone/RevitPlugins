using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class DiagonalValueGetter : IValueGetter<DoubleParamValue> {
        private readonly MepCurveClash<Wall> _clash;
        private readonly Plane _plane;
        private readonly MepCategory _categoryOptions;

        public DiagonalValueGetter(MepCurveClash<Wall> clash, Plane plane, MepCategory categoryOptions) {
            _clash = clash;
            _plane = plane;
            _categoryOptions = categoryOptions;
        }

        public DoubleParamValue GetValue() {
            var height = _clash.Element1.GetHeight();
            var width = _clash.Element1.GetWidth();
            var coordinateSystem = _clash.Element1.GetConnectorCoordinateSystem();
            var dirX = coordinateSystem.BasisX;
            var dirY = coordinateSystem.BasisY;

            height += _categoryOptions.GetOffsetValue(height);
            width += _categoryOptions.GetOffsetValue(width);

            //получение длин проекций диагоналей коннектора инженерной системы на плоскость
            var diagonals = new[] { _plane.ProjectVector(dirX * width + dirY * height).GetLength(),
                                    _plane.ProjectVector(dirX * width - dirY * height).GetLength() };

            return new DoubleParamValue(diagonals.Max());
        }
    }
}
