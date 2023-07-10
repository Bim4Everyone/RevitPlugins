
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class FloorThicknessValueGetter : IValueGetter<DoubleParamValue> {
        private readonly MepCurveClash<CeilingAndFloor> _clash;

        /// <summary>
        /// Минимальное значение габарита задания на отверстие в футах (5 мм)
        /// </summary>
        private const double _minGeometryFeetSize = 0.015;

        public FloorThicknessValueGetter(MepCurveClash<CeilingAndFloor> clash) {
            _clash = clash;
        }

        public DoubleParamValue GetValue() {
            var thickness = _clash.Element2.GetThickness();
            //проверка на недопустимо малые габариты
            if(thickness < _minGeometryFeetSize) {
                throw new SizeTooSmallException("Заданный габарит отверстия слишком мал");
            }
            return new DoubleParamValue(thickness);
        }
    }
}
