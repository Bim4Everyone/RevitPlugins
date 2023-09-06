
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class DiameterValueGetter : RoundValueGetter, IValueGetter<DoubleParamValue> {
        private readonly MEPCurve _curve;
        private readonly MepCategory _categoryOptions;

        /// <summary>
        /// Минимальное значение габарита задания на отверстие в футах (5 мм)
        /// </summary>
        private const double _minGeometryFeetSize = 0.015;

        public DiameterValueGetter(MEPCurve curve, MepCategory categoryOptions) {
            _curve = curve;
            _categoryOptions = categoryOptions;
        }

        public DoubleParamValue GetValue() {
            var diameter = _curve.GetDiameter();
            diameter += _categoryOptions.GetOffsetValue(diameter);
            var roundDiameter = RoundFeetToMillimeters(diameter, _categoryOptions.Rounding);

            //проверка на недопустимо малые габариты
            if(roundDiameter < _minGeometryFeetSize) {
                throw new SizeTooSmallException("Заданный габарит отверстия слишком мал");
            }

            return new DoubleParamValue(roundDiameter);
        }
    }
}
