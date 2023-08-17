using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningPlacement.ValueGetters {
    /// <summary>
    /// Класс, предоставляющий значение высоты прямоугольного задания на отверстие в стене в единицах ревита
    /// </summary>
    internal class RectangleOpeningTaskInWallHeightValueGetter : RoundValueGetter, IValueGetter<DoubleParamValue> {
        private readonly OpeningMepTaskIncoming _openingMepTaskIncoming;
        private readonly IPointFinder _pointFinder;

        /// <summary>
        /// Значение округления в мм
        /// </summary>
        private const int _heightRound = 10;


        /// <summary>
        /// Конструктор класса, предоставляющего значение высоты прямоугольного задания на отверстие в стене в единицах ревита
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <param name="pointFinder">Провайдер точки вставки чистового отверстия</param>
        public RectangleOpeningTaskInWallHeightValueGetter(OpeningMepTaskIncoming incomingTask, IPointFinder pointFinder) {
            if(incomingTask is null) { throw new ArgumentNullException(nameof(incomingTask)); }
            if(pointFinder is null) { throw new ArgumentNullException(nameof(pointFinder)); }

            _openingMepTaskIncoming = incomingTask;
            _pointFinder = pointFinder;
        }


        public DoubleParamValue GetValue() {
            double heightFeet = _openingMepTaskIncoming.Height;
            double zOffset = Math.Abs(_openingMepTaskIncoming.Location.Z - _pointFinder.GetPoint().Z);
            double heightWithOffset = heightFeet + zOffset;

            double roundHeightFeet = RoundToCeilingFeetToMillimeters(heightWithOffset, _heightRound);
            return new DoubleParamValue(roundHeightFeet);
        }
    }
}
