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
        private readonly int _rounding;


        /// <summary>
        /// Конструктор класса, предоставляющего <see cref="IParametersGetter"/> для чистового отверстия КР, размещаемого по одному заданию на отверстие
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <param name="pointFinder">Провайдер точки вставки отверстия КР</param>
        /// <param name="rounding">Округление размеров отверстия в мм</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        public SingleOpeningArTaskParameterGettersProvider(IOpeningTaskIncoming incomingTask, IPointFinder pointFinder, int rounding) {
            _incomingTask = incomingTask ?? throw new ArgumentNullException(nameof(incomingTask));
            _pointFinder = pointFinder ?? throw new ArgumentNullException(nameof(pointFinder));
            _rounding = rounding;
        }


        public IParametersGetter GetParametersGetter() {
            switch(_incomingTask.OpeningType) {
                case OpeningType.WallRound:
                    return new SingleRoundOpeningArTaskInWallParameterGetter(_incomingTask, _pointFinder, _rounding);

                case OpeningType.WallRectangle:
                    return new SingleRectangleOpeningArTaskInWallParameterGetter(_incomingTask, _pointFinder, _rounding);

                case OpeningType.FloorRound:
                case OpeningType.FloorRectangle:
                    return new SingleOpeningArTaskInFloorParameterGetter(_incomingTask, _rounding);

                default:
                    throw new ArgumentException(nameof(_incomingTask.OpeningType));
            }
        }
    }
}
