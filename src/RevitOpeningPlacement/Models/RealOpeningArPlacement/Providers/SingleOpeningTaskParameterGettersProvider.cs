using System;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningArPlacement.ParameterGetters;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.Providers {
    /// <summary>
    /// Класс, предоставляющий <see cref="IParametersGetter"/> для чистового отверстия для размещения по одному заданию на отверстие
    /// </summary>
    internal class SingleOpeningTaskParameterGettersProvider {
        private readonly OpeningMepTaskIncoming _openingMepTaskIncoming;
        private readonly IPointFinder _pointFinder;


        /// <summary>
        /// Конструктор класса, предоставляющего <see cref="IParametersGetter"/> для чистового отверстия для размещения по одному заданию на отверстие
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <param name="pointFinder">Провайдер точки вставки чистового отверстия</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SingleOpeningTaskParameterGettersProvider(OpeningMepTaskIncoming incomingTask, IPointFinder pointFinder) {
            if(incomingTask is null) { throw new ArgumentNullException(nameof(incomingTask)); }
            if(pointFinder is null) { throw new ArgumentNullException(nameof(pointFinder)); }

            _openingMepTaskIncoming = incomingTask;
            _pointFinder = pointFinder;
        }


        /// <exception cref="ArgumentException"></exception>
        public IParametersGetter GetParametersGetter() {
            switch(_openingMepTaskIncoming.OpeningType) {
                case OpeningType.WallRectangle:
                return new SingleRectangleOpeningTaskInWallParameterGetter(_openingMepTaskIncoming, _pointFinder);
                case OpeningType.FloorRectangle:
                return new SingleRectangleOpeningTaskInFloorParameterGetter(_openingMepTaskIncoming);
                case OpeningType.WallRound:
                return new SingleRoundOpeningTaskInWallParameterGetter(_openingMepTaskIncoming, _pointFinder);
                case OpeningType.FloorRound:
                return new SingleRoundOpeningTaskInFloorParameterGetter(_openingMepTaskIncoming);
                default:
                throw new ArgumentException(nameof(_openingMepTaskIncoming.OpeningType));
            }
        }
    }
}
