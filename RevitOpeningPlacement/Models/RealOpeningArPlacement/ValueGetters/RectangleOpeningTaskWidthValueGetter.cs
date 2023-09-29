using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.ValueGetters {
    /// <summary>
    /// Класс, предоставляющий значение ширины прямоугольного задания на отверстие в единицах ревита
    /// </summary>
    internal class RectangleOpeningTaskWidthValueGetter : RoundValueGetter, IValueGetter<DoubleParamValue> {
        private readonly OpeningMepTaskIncoming _openingMepTaskIncoming;
        /// <summary>
        /// Значение округления в мм
        /// </summary>
        private const int _widthRound = 10;

        /// <summary>
        /// Конструктор класса, предоставляющего значение ширины прямоугольного задания на отверстие в единицах ревита
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        public RectangleOpeningTaskWidthValueGetter(OpeningMepTaskIncoming incomingTask) {
            if(incomingTask is null) { throw new ArgumentNullException(nameof(incomingTask)); }

            _openingMepTaskIncoming = incomingTask;
        }



        public DoubleParamValue GetValue() {
            double heightFeet = _openingMepTaskIncoming.Width;
            double roundHeightFeet = RoundToCeilingFeetToMillimeters(heightFeet, _widthRound);
            return new DoubleParamValue(roundHeightFeet);
        }
    }
}
