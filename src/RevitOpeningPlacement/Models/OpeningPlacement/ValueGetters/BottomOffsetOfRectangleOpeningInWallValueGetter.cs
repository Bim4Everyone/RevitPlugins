using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class BottomOffsetOfRectangleOpeningInWallValueGetter : LengthConverter, IValueGetter<DoubleParamValue> {
        private readonly IPointFinder _pointFinder;

        /// <summary>
        /// Конструктор класса, предоставляющего значение высотной отметки низа прямоугольного отверстия в стене в мм от начала проекта.
        /// Класс создан для расчета высотной отметки оси и низа отверстия от нуля для семейства задания прямоугольного отверстия в стене, у которого точка размещения находится у нижней грани.
        /// </summary>
        /// <param name="pointFinder">Объект, предоставляющий координату центра отверстия в футах</param>
        /// <exception cref="ArgumentNullException"></exception>
        public BottomOffsetOfRectangleOpeningInWallValueGetter(IPointFinder pointFinder) {
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
