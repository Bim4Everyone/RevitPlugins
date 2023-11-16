using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RealOpeningsGeometryValueGetters {
    /// <summary>
    /// Класс, предоставляющий значение диаметра чистового отверстия АР/КР в стене в единицах Revit
    /// </summary>
    internal class RoundOpeningInWallDiameterValueGetter : RealOpeningSizeValueGetter, IValueGetter<DoubleParamValue> {
        private readonly IOpeningTaskIncoming _openingTaskIncoming;
        private readonly IPointFinder _pointFinder;


        /// <summary>
        /// Конструктор класса, предоставляющего значение диаметра чистового отверстия АР/КР в стене в единицах Revit
        /// </summary>
        /// <param name="openingTaskIncoming">Входящее задание на отверстие</param>
        /// <param name="pointFinder">Провайдер точки вставки чистового отверстия АР/КР</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RoundOpeningInWallDiameterValueGetter(IOpeningTaskIncoming openingTaskIncoming, IPointFinder pointFinder) {
            _openingTaskIncoming = openingTaskIncoming ?? throw new ArgumentNullException(nameof(openingTaskIncoming));
            _pointFinder = pointFinder ?? throw new ArgumentNullException(nameof(pointFinder));
        }


        public DoubleParamValue GetValue() {
            double diameterFeet = GetOpeningDiameter(_openingTaskIncoming);
            double zOffset = Math.Abs(_openingTaskIncoming.Location.Z - _pointFinder.GetPoint().Z);
            double diameterWithOffset = diameterFeet + 2 * zOffset;

            double roundHeightFeet = RoundToCeilingFeetToMillimeters(diameterWithOffset, _diameterRound);
            return new DoubleParamValue(roundHeightFeet);
        }
    }
}
