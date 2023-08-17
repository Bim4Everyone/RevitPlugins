using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningPlacement.ValueGetters {
    /// <summary>
    /// Класс, предоставляющий значение диаметра круглого задания на отверстие в стене в единицах ревита
    /// </summary>
    internal class RoundOpeningTaskInWallDiameterValueGetter : RoundValueGetter, IValueGetter<DoubleParamValue> {
        private readonly OpeningMepTaskIncoming _openingMepTaskIncoming;
        private readonly IPointFinder _pointFinder;

        /// <summary>
        /// Значение округления в мм
        /// </summary>
        private const int _diameterRound = 10;

        /// <summary>
        /// Конструктор класса, предоставляющего значение диаметра круглого задания на отверстие в стене в единицах ревита
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <param name="pointFinder">Провайдер точки вставки чистового отверстия</param>
        public RoundOpeningTaskInWallDiameterValueGetter(OpeningMepTaskIncoming incomingTask, IPointFinder pointFinder) {
            if(incomingTask is null) { throw new ArgumentNullException(nameof(incomingTask)); }
            if(pointFinder is null) { throw new ArgumentNullException(nameof(pointFinder)); }

            _openingMepTaskIncoming = incomingTask;
            _pointFinder = pointFinder;
        }


        public DoubleParamValue GetValue() {
            double diameterFeet = _openingMepTaskIncoming.Diameter;
            double zOffset = Math.Abs(_openingMepTaskIncoming.Location.Z - _pointFinder.GetPoint().Z);
            double diameterWithOffset = diameterFeet + 2 * zOffset;

            double roundHeightFeet = RoundToCeilingFeetToMillimeters(diameterWithOffset, _diameterRound);
            return new DoubleParamValue(roundHeightFeet);
        }
    }
}
