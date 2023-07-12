
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class WallThicknessValueGetter : IValueGetter<DoubleParamValue> {
        private readonly MepCurveClash<Wall> _clash;

        /// <summary>
        /// Минимальное значение габарита задания на отверстие в футах (5 мм)
        /// </summary>
        private const double _minGeometryFeetSize = 0.015;

        public WallThicknessValueGetter(MepCurveClash<Wall> clash) {
            _clash = clash;
        }

        public DoubleParamValue GetValue() {
            //проверка на недопустимо малые габариты
            if(_clash.Element2.Width < _minGeometryFeetSize) {
                throw new SizeTooSmallException("Заданный габарит отверстия слишком мал");
            }

            return new DoubleParamValue(_clash.Element2.Width);
        }
    }
}
