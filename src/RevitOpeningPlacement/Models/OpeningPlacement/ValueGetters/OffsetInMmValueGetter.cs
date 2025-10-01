using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
internal class OffsetInMmValueGetter : LengthConverter, IValueGetter<DoubleParamValue> {
    private readonly IValueGetter<DoubleParamValue> _offsetInFeetValueGetter;

    /// <summary>
    /// Класс, предоставляющий значение отметки отверстия в мм
    /// </summary>
    /// <param name="offsetInFeetValueGetter">Провайдер отметки отверстия в футах</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public OffsetInMmValueGetter(IValueGetter<DoubleParamValue> offsetInFeetValueGetter) {
        _offsetInFeetValueGetter = offsetInFeetValueGetter ?? throw new ArgumentNullException(nameof(offsetInFeetValueGetter));
    }


    public DoubleParamValue GetValue() {
        return new DoubleParamValue(ConvertFromInternal(_offsetInFeetValueGetter.GetValue().TValue));
    }
}
