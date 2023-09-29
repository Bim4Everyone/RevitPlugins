using System;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.ValueGetters {
    /// <summary>
    /// Класс, предоставляющий значение диаметра круглого задания на отверстие в перекрытии в единицах ревита
    /// </summary>
    internal class RoundOpeningTaskInFloorDiameterValueGetter : RoundValueGetter, IValueGetter<DoubleParamValue> {
        private readonly OpeningMepTaskIncoming _openingMepTaskIncoming;
        /// <summary>
        /// Значение округления в мм
        /// </summary>
        private const int _diameterRound = 10;

        /// <summary>
        /// Конструктор класса, предоставляющего значение диаметра круглого задания на отверстие в перекрытии в единицах ревита
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        public RoundOpeningTaskInFloorDiameterValueGetter(OpeningMepTaskIncoming incomingTask) {
            if(incomingTask is null) { throw new ArgumentNullException(nameof(incomingTask)); }

            _openingMepTaskIncoming = incomingTask;
        }


        public DoubleParamValue GetValue() {
            double heightFeet = _openingMepTaskIncoming.Diameter;
            double roundHeightFeet = RoundToCeilingFeetToMillimeters(heightFeet, _diameterRound);
            return new DoubleParamValue(roundHeightFeet);
        }
    }
}
