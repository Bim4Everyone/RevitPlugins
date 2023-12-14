using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.PointFinders {
    /// <summary>
    /// Класс, предоставляющий точку вставки для чистового отверстия КР
    /// </summary>
    internal class SingleOpeningArTaskPointFinder : RoundValueGetter, IPointFinder {
        private readonly IOpeningTaskIncoming _openingArTaskIncoming;


        /// <summary>
        /// Конструктор класса, предоставляющего точку вставки для чистового отверстия КР
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public SingleOpeningArTaskPointFinder(IOpeningTaskIncoming incomingTask) {
            _openingArTaskIncoming = incomingTask ?? throw new System.ArgumentNullException(nameof(incomingTask));
        }


        public XYZ GetPoint() {
            switch(_openingArTaskIncoming.OpeningType) {
                case OpeningType.WallRound:
                case OpeningType.WallRectangle:
                var point = _openingArTaskIncoming.Location;
                return new XYZ(point.X, point.Y, RoundToFloorFeetToMillimeters(point.Z, 10));
                default:
                return _openingArTaskIncoming.Location;
            }
        }
    }
}
