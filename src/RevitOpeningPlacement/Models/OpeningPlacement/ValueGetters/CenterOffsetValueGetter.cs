using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class CenterOffsetValueGetter : LengthConverter, IValueGetter<DoubleParamValue> {
        private readonly IPointFinder _pointFinder;

        /// <summary>
        /// Конструктор класса, предоставляющего значение высотной отметки центра отверстия в мм<br/>
        /// от начала проекта для круглых отверстиях в стенах и перекрытиях и для прямоугольных отверстий в перекрытиях<br/>
        /// Для прямоугольных отверстий в стенах использовать <see cref="CenterOffsetOfRectangleOpeningInWallValueGetter"/>
        /// </summary>
        /// <param name="pointFinder">Объект, предоставляющий координату центра отверстия</param>
        /// <exception cref="ArgumentNullException">Исключение, если <paramref name="pointFinder"/> null</exception>
        public CenterOffsetValueGetter(IPointFinder pointFinder) {
            if(pointFinder is null) {
                throw new ArgumentNullException(nameof(pointFinder));
            }
            _pointFinder = pointFinder;
        }


        public DoubleParamValue GetValue() {
            var offsetInFeet = _pointFinder.GetPoint().Z;
            return new DoubleParamValue(Math.Round(ConvertFromInternal(offsetInFeet)));
        }
    }
}
