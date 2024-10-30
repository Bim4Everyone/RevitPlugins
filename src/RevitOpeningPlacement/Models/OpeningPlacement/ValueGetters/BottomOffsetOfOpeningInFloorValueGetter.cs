using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class BottomOffsetOfOpeningInFloorValueGetter : LengthConverter, IValueGetter<DoubleParamValue> {
        private readonly IPointFinder _pointFinder;

        /// <summary>
        /// Конструктор класса, предоставляющего значение высотной отметки низа отверстия в мм от начала проекта, расположенного в перекрытии
        /// </summary>
        /// <param name="pointFinder">Объект, предоставляющий координату центра отверстия в футах</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        public BottomOffsetOfOpeningInFloorValueGetter(IPointFinder pointFinder) {
            if(pointFinder is null) {
                throw new ArgumentNullException(nameof(pointFinder));
            }
            _pointFinder = pointFinder;
        }

        public DoubleParamValue GetValue() {
            double offsetInFeet = _pointFinder.GetPoint().Z;
            double offsetInMm = ConvertFromInternal(offsetInFeet);
            return new DoubleParamValue(Math.Round(offsetInMm));
        }
    }
}
