using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
internal class CenterOffsetValueGetter : LengthConverter, IValueGetter<DoubleParamValue> {
    private readonly IValueGetter<DoubleParamValue> _elevationGetter;

    /// <summary>
    /// Конструктор класса, предоставляющего значение высотной отметки центра отверстия в мм<br/>
    /// для круглых отверстиях в стенах и перекрытиях и для прямоугольных отверстий в перекрытиях<br/>
    /// Для прямоугольных отверстий в стенах использовать <see cref="CenterOffsetOfRectangleOpeningInWallValueGetter"/>
    /// </summary>
    /// <param name="elevationGetter">Объект, предоставляющий Z координату центра отверстия</param>
    /// <exception cref="ArgumentNullException">Исключение, если <paramref name="elevationGetter"/> null</exception>
    public CenterOffsetValueGetter(IValueGetter<DoubleParamValue> elevationGetter) {
        _elevationGetter = elevationGetter ?? throw new ArgumentNullException(nameof(elevationGetter));
    }


    public DoubleParamValue GetValue() {
        double offsetInFeet = _elevationGetter.GetValue().TValue;
        return new DoubleParamValue(Math.Round(ConvertFromInternal(offsetInFeet)));
    }
}
