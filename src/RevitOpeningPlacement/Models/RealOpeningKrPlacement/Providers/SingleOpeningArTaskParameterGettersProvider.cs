using System;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningKrPlacement.ParameterGetters;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.Providers {
    /// <summary>
    /// Класс, предоставляющий <see cref="IParametersGetter"/> для чистового отверстия КР, размещаемого по одному заданию на отверстие
    /// </summary>
    internal class SingleOpeningArTaskParameterGettersProvider {
        private readonly IOpeningTaskIncoming _incomingTask;
        private readonly IPointFinder _pointFinder;


        /// <summary>
        /// Конструктор класса, предоставляющего <see cref="IParametersGetter"/> для чистового отверстия КР, размещаемого по одному заданию на отверстие
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <param name="pointFinder">Провайдер точки вставки отверстия КР</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SingleOpeningArTaskParameterGettersProvider(IOpeningTaskIncoming incomingTask, IPointFinder pointFinder) {
            _incomingTask = incomingTask ?? throw new ArgumentNullException(nameof(incomingTask));
            _pointFinder = pointFinder ?? throw new ArgumentNullException(nameof(pointFinder));
        }


        public IParametersGetter GetParametersGetter() {
            switch(_incomingTask.OpeningType) {
                case OpeningType.WallRound:
                return new SingleRoundOpeningArTaskInWallParameterGetter(_incomingTask, _pointFinder);

                case OpeningType.WallRectangle:
                return new SingleRectangleOpeningArTaskInWallParameterGetter(_incomingTask, _pointFinder);

                case OpeningType.FloorRound:
                case OpeningType.FloorRectangle:
                return new SingleOpeningArTaskInFloorParameterGetter(_incomingTask);

                default:
                throw new ArgumentException(nameof(_incomingTask.OpeningType));
            }
        }
    }
}
