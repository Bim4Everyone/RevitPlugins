using System;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.PointFinders;
/// <summary>
/// Класс, предоставляющий точку вставки для чистового отверстия по заданию на отверстие
/// </summary>
internal class SingleOpeningTaskPointFinder : RoundValueGetter, IPointFinder {
    private readonly OpeningMepTaskIncoming _openingMepTaskIncoming;
    private readonly int _rounding;

    /// <summary>
    /// Конструктор класса, предоставляющего точку вставки для чистового отверстия по заданию на отверстие
    /// </summary>
    /// <param name="incomingTask">Входящее задание на отверстие</param>
    /// <param name="rounding">Округление высотной отметки в мм</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public SingleOpeningTaskPointFinder(OpeningMepTaskIncoming incomingTask, int rounding) {
        _openingMepTaskIncoming = incomingTask ?? throw new ArgumentNullException(nameof(incomingTask));
        _rounding = rounding;
    }


    public XYZ GetPoint() {
        switch(_openingMepTaskIncoming.OpeningType) {
            case OpeningType.WallRound:
            case OpeningType.WallRectangle:
                var point = _openingMepTaskIncoming.Location;
                return new XYZ(point.X, point.Y, RoundToFloorFeetToMillimeters(point.Z, _rounding));
            default:
                return _openingMepTaskIncoming.Location;
        }
    }
}
