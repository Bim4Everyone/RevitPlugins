using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.RealOpeningPlacement.ValueGetters {
    /// <summary>
    /// Класс, предоставляющий округленное значение длины в футах
    /// </summary>
    internal class DimensionValueGetter : RoundValueGetter, IValueGetter<DoubleParamValue> {
        /// <summary>
        /// Значение округления в мм
        /// </summary>
        private const int _dimensionRound = 10;

        /// <summary>
        /// Минимальное значение габарита в футах (5 мм)
        /// </summary>
        private const double _minGeometryFeetSize = 0.015;

        private readonly double _feetDimensionValue;


        /// <summary>
        /// Конструктор класса, предоставляющего округленное значение длины в футах
        /// </summary>
        /// <param name="feetDimensionValue">Значение длины в футах</param>
        /// <exception cref="ArgumentException"></exception>
        public DimensionValueGetter(double feetDimensionValue) {
            if(feetDimensionValue < _minGeometryFeetSize) { throw new ArgumentException("Заданный габарит меньше 5 мм"); }

            _feetDimensionValue = feetDimensionValue;
        }

        public DoubleParamValue GetValue() {
            return new DoubleParamValue(RoundToCeilingFeetToMillimeters(_feetDimensionValue, _dimensionRound));
        }
    }
}
