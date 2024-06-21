using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class BottomOffsetInFeetValueGetter : LengthConverter, IValueGetter<DoubleParamValue> {
        private readonly IValueGetter<DoubleParamValue> _bottomOffsetInMmValueGetter;

        /// <summary>
        /// Класс, предоставляющий значение отметки низа отверстия в футах от начала проекта
        /// </summary>
        /// <param name="bottomOffsetInMmValueGetter">Провайдер отметки низа отверстия в мм от начала проекта</param>
        /// <exception cref="ArgumentNullException"></exception>
        public BottomOffsetInFeetValueGetter(IValueGetter<DoubleParamValue> bottomOffsetInMmValueGetter) {
            _bottomOffsetInMmValueGetter = bottomOffsetInMmValueGetter ?? throw new ArgumentNullException(nameof(bottomOffsetInMmValueGetter));
        }


        public DoubleParamValue GetValue() {
            return new DoubleParamValue(ConvertToInternal(_bottomOffsetInMmValueGetter.GetValue().TValue));
        }
    }
}
