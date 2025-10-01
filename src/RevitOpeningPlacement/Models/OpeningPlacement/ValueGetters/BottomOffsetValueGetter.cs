using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
internal class BottomOffsetValueGetter : LengthConverter, IValueGetter<DoubleParamValue> {
    private readonly IValueGetter<DoubleParamValue> _elevationGetter;
    private readonly IValueGetter<DoubleParamValue> _heightInFeetParamGetter;

    /// <summary>
    /// Конструктор класса, предоставляющего значение высотной отметки низа отверстия в мм для круглых отверстий в стенах
    /// Для прямоугольных отверстий в стенах использовать <see cref="BottomOffsetOfRectangleOpeningInWallValueGetter"/>
    /// Для отверстий в перекрытиях использовать <see cref="BottomOffsetOfOpeningInFloorValueGetter"/>
    /// </summary>
    /// <param name="elevationGetter">Объект, предоставляющий координату Z центра отверстия в футах</param>
    /// <param name="heightInFeetParamGetter">Объект, предоставляющий высоту отверстия в футах</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public BottomOffsetValueGetter(
        IValueGetter<DoubleParamValue> elevationGetter,
        IValueGetter<DoubleParamValue> heightInFeetParamGetter) {

        _elevationGetter = elevationGetter
            ?? throw new ArgumentNullException(nameof(elevationGetter));
        _heightInFeetParamGetter = heightInFeetParamGetter
            ?? throw new ArgumentNullException(nameof(heightInFeetParamGetter));
    }

    public DoubleParamValue GetValue() {
        double offsetInFeet = _elevationGetter.GetValue().TValue - _heightInFeetParamGetter.GetValue().TValue / 2;
        double offsetInMm = ConvertFromInternal(offsetInFeet);
        return new DoubleParamValue(Math.Round(offsetInMm));
    }
}
