using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.ValueGetters {
    /// <summary>
    /// Класс, предоставляющий значение высоты прямоугольного задания на отверстие в перекрытии в единицах ревита
    /// </summary>
    internal class RectangleOpeningTaskInFloorHeightValueGetter : RoundValueGetter, IValueGetter<DoubleParamValue> {
        private readonly OpeningMepTaskIncoming _openingMepTaskIncoming;
        /// <summary>
        /// Значение округления в мм
        /// </summary>
        private const int _heightRound = 10;


        /// <summary>
        /// Конструктор класса, предоставляющего значение высоты прямоугольного задания на отверстие в перекрытии в единицах ревита
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        public RectangleOpeningTaskInFloorHeightValueGetter(OpeningMepTaskIncoming incomingTask) {
            if(incomingTask is null) { throw new ArgumentNullException(nameof(incomingTask)); }

            _openingMepTaskIncoming = incomingTask;
        }


        public DoubleParamValue GetValue() {
            double heightFeet = _openingMepTaskIncoming.Height;
            double roundHeightFeet = RoundToCeilingFeetToMillimeters(heightFeet, _heightRound);
            return new DoubleParamValue(roundHeightFeet);
        }
    }
}
