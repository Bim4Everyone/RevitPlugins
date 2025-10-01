using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
internal class OffsetFromLevelValueGetter : LengthConverter, IValueGetter<DoubleParamValue> {
    /// <summary>
    /// Провайдер отметки низа отверстия в мм
    /// </summary>
    private readonly IValueGetter<DoubleParamValue> _offsetValueGetter;
    private readonly ILevelFinder _levelFinder;

    /// <summary>
    /// Класс, предоставляющий значение отметки низа отверстия от уровня в единицах Revit
    /// </summary>
    /// <param name="offsetMmValueGetter">Провайдер отметки низа отверстия в мм</param>
    /// <param name="levelFinder">Провайдер уровня, на котором размещается отверстие</param>
    /// <exception cref="System.ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public OffsetFromLevelValueGetter(
        IValueGetter<DoubleParamValue> offsetMmValueGetter,
        ILevelFinder levelFinder) {

        _offsetValueGetter = offsetMmValueGetter
            ?? throw new System.ArgumentNullException(nameof(offsetMmValueGetter));
        _levelFinder = levelFinder
            ?? throw new System.ArgumentNullException(nameof(levelFinder));
    }


    public DoubleParamValue GetValue() {
        double offset = ConvertToInternal(_offsetValueGetter.GetValue().TValue);
        double levelOffset = _levelFinder.GetLevel().Elevation;
        return new DoubleParamValue(offset - levelOffset);
    }
}
