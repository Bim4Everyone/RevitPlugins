using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.PointFinders;
/// <summary>
/// Класс, предоставляющий точку вставки для чистового отверстия КР
/// </summary>
internal class SingleOpeningArTaskPointFinder : RoundValueGetter, IPointFinder {
    private readonly IOpeningTaskIncoming _openingArTaskIncoming;
    private readonly int _rounding;


    /// <summary>
    /// Конструктор класса, предоставляющего точку вставки для чистового отверстия КР
    /// </summary>
    /// <param name="incomingTask">Входящее задание на отверстие</param>
    /// <param name="rounding">Округление отметки отверстия в мм</param>
    /// <exception cref="System.ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public SingleOpeningArTaskPointFinder(IOpeningTaskIncoming incomingTask, int rounding) {
        _openingArTaskIncoming = incomingTask ?? throw new System.ArgumentNullException(nameof(incomingTask));
        _rounding = rounding;
    }


    public XYZ GetPoint() {
        switch(_openingArTaskIncoming.OpeningType) {
            case OpeningType.WallRound:
            case OpeningType.WallRectangle:
                var point = _openingArTaskIncoming.Location;
                return new XYZ(point.X, point.Y, RoundToFloorFeetToMillimeters(point.Z, _rounding));
            default:
                return _openingArTaskIncoming.Location;
        }
    }
}
