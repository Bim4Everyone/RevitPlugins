using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RealOpeningsGeometryValueGetters;
/// <summary>
/// Класс, предоставляющий значение диаметра чистового отверстия АР/КР в перекрытии в единицах Revit
/// </summary>
internal class RoundOpeningInFloorDiameterValueGetter : RealOpeningSizeValueGetter, IValueGetter<DoubleParamValue> {
    private readonly IOpeningTaskIncoming _openingTaskIncoming;


    /// <summary>
    /// Конструктор класса, предоставляющего значение диаметра чистового отверстия АР/КР в перекрытии в единицах Revit
    /// </summary>
    /// <param name="openingTaskIncoming">Входящее задание на отверстие</param>
    /// <param name="rounding">Округление размеров отверстия в мм</param>
    /// <exception cref="System.ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public RoundOpeningInFloorDiameterValueGetter(IOpeningTaskIncoming openingTaskIncoming, int rounding)
        : base(rounding, rounding, rounding) {
        _openingTaskIncoming = openingTaskIncoming ?? throw new System.ArgumentNullException(nameof(openingTaskIncoming));
    }


    public DoubleParamValue GetValue() {
        double diameterFeet = GetOpeningDiameter(_openingTaskIncoming);
        double roundDiameterFeet = RoundToCeilingFeetToMillimeters(diameterFeet, _diameterRound);
        return new DoubleParamValue(roundDiameterFeet);
    }
}
