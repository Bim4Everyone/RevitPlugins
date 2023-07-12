
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class InclinedSizeValueGetter : RoundValueGetter, IValueGetter<DoubleParamValue> {
        private readonly MepCurveClash<Wall> _clash;
        private readonly Plane _plane;
        private readonly IValueGetter<DoubleParamValue> _sizeValueGetter;
        private readonly IDirectionsGetter _directionsGetter;
        private readonly MepCategory _mepCategory;

        /// <summary>
        /// Минимальное значение габарита задания на отверстие в футах (5 мм)
        /// </summary>
        private const double _minGeometryFeetSize = 0.015;

        public InclinedSizeValueGetter(
            MepCurveClash<Wall> clash,
            IValueGetter<DoubleParamValue> sizeValueGetter,
            Plane plane,
            IDirectionsGetter directionsGetter,
            MepCategory mepCategory) {

            _clash = clash;
            _sizeValueGetter = sizeValueGetter;
            _plane = plane;
            _directionsGetter = directionsGetter;
            _mepCategory = mepCategory;
        }

        public DoubleParamValue GetValue() {
            var size = GetSize();
            var roundSize = RoundFeetToMillimeters(size, _mepCategory.Rounding);
            //проверка на недопустимо малые габариты
            if(roundSize < _minGeometryFeetSize) {
                throw new SizeTooSmallException("Заданный габарит отверстия слишком мал");
            }
            return new DoubleParamValue(roundSize);
        }

        private double GetSize() {
            //получение длины размера, спроецированного на плоскость
            var size = _sizeValueGetter.GetValue().TValue / 2;
            var directions = _directionsGetter.GetDirectionsOnPlane(_plane);
            return new SizeGetter(_clash, _plane).GetSizeFromProjection(directions, size);
        }
    }
}
