using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class BottomOffsetOfOpeningInFloorValueGetter : LengthConverter, IValueGetter<DoubleParamValue> {
        private readonly IPointFinder _pointFinder;
        private readonly IValueGetter<DoubleParamValue> _heightInFeetParamGetter;

        /// <summary>
        /// Конструктор класса, предоставляющего значение высотной отметки низа отверстия в мм от начала проекта, расположенного в перекрытии
        /// </summary>
        /// <param name="pointFinder">Объект, предоставляющий координату центра отверстия в футах</param>
        /// <param name="heightInFeetParamGetter">Объект, предоставляющий высоту отверстия в футах</param>
        /// <exception cref="ArgumentNullException"></exception>
        public BottomOffsetOfOpeningInFloorValueGetter(IPointFinder pointFinder, IValueGetter<DoubleParamValue> heightInFeetParamGetter) {
            if(pointFinder is null) {
                throw new ArgumentNullException(nameof(pointFinder));
            }
            if(heightInFeetParamGetter is null) {
                throw new ArgumentNullException(nameof(heightInFeetParamGetter));
            }
            _pointFinder = pointFinder;
            _heightInFeetParamGetter = heightInFeetParamGetter;
        }

        public DoubleParamValue GetValue() {
            double offsetInFeet = _pointFinder.GetPoint().Z - _heightInFeetParamGetter.GetValue().TValue;
            double offsetInMm = ConvertFromInternal(offsetInFeet);
            return new DoubleParamValue(Math.Round(offsetInMm));
        }
    }
}
