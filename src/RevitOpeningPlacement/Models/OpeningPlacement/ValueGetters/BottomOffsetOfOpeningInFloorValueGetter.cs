using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
internal class BottomOffsetOfOpeningInFloorValueGetter : LengthConverter, IValueGetter<DoubleParamValue> {
    private readonly IValueGetter<DoubleParamValue> _elevationGetter;
    private readonly IValueGetter<DoubleParamValue> _thicknessInFeetParamGetter;

    /// <summary>
    /// Конструктор класса, предоставляющего значение высотной отметки низа отверстия в мм, расположенного в перекрытии
    /// </summary>
    /// <param name="elevationGetter">Объект, предоставляющий координату Z центра отверстия в футах</param>
    /// <param name="thicknessInFeetParamGetter">Объект, предоставляющий толщину отверстия в футах</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public BottomOffsetOfOpeningInFloorValueGetter(
        IValueGetter<DoubleParamValue> elevationGetter,
        IValueGetter<DoubleParamValue> thicknessInFeetParamGetter) {

        _elevationGetter = elevationGetter
            ?? throw new ArgumentNullException(nameof(elevationGetter));
        _thicknessInFeetParamGetter = thicknessInFeetParamGetter
            ?? throw new ArgumentNullException(nameof(thicknessInFeetParamGetter));
    }

    public DoubleParamValue GetValue() {
        double offsetInFeet = _elevationGetter.GetValue().TValue - _thicknessInFeetParamGetter.GetValue().TValue;
        double offsetInMm = ConvertFromInternal(offsetInFeet);
        return new DoubleParamValue(Math.Round(offsetInMm));
    }
}
