using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
internal class BottomOffsetOfRectangleOpeningInWallValueGetter : LengthConverter, IValueGetter<DoubleParamValue> {
    private readonly IValueGetter<DoubleParamValue> _elevationGetter;

    /// <summary>
    /// Конструктор класса, предоставляющего значение высотной отметки низа прямоугольного отверстия в стене в мм.
    /// Класс создан для расчета высотной отметки оси и низа отверстия от нуля для семейства задания прямоугольного отверстия в стене, у которого точка размещения находится у нижней грани.
    /// </summary>
    /// <param name="elevationGetter">Объект, предоставляющий координату Z центра отверстия в футах</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public BottomOffsetOfRectangleOpeningInWallValueGetter(IValueGetter<DoubleParamValue> elevationGetter) {
        _elevationGetter = elevationGetter ?? throw new ArgumentNullException(nameof(elevationGetter));
    }

    public DoubleParamValue GetValue() {
        double offsetInFeet = _elevationGetter.GetValue().TValue;
        double offsetInMm = ConvertFromInternal(offsetInFeet);
        return new DoubleParamValue(Math.Round(offsetInMm));
    }
}
