using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
internal class OffsetInFeetValueGetter : LengthConverter, IValueGetter<DoubleParamValue> {
    private readonly IValueGetter<DoubleParamValue> _offsetInMmValueGetter;

    /// <summary>
    /// Класс, предоставляющий значение отметки отверстия в футах
    /// </summary>
    /// <param name="offsetInMmValueGetter">Провайдер отметки отверстия в мм</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public OffsetInFeetValueGetter(IValueGetter<DoubleParamValue> offsetInMmValueGetter) {
        _offsetInMmValueGetter = offsetInMmValueGetter ?? throw new ArgumentNullException(nameof(offsetInMmValueGetter));
    }


    public DoubleParamValue GetValue() {
        return new DoubleParamValue(ConvertToInternal(_offsetInMmValueGetter.GetValue().TValue));
    }
}
