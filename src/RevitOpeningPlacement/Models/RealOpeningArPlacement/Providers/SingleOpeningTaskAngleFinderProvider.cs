using System;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.Models.RealOpeningArPlacement.AngleFinders;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.Providers {
    /// <summary>
    /// Класс, предоставляющий <see cref="IAngleFinder"/> для чистового отверстия для размещения по одному заданию на отверстие
    /// </summary>
    internal class SingleOpeningTaskAngleFinderProvider {
        private readonly OpeningMepTaskIncoming _openingMepTaskIncoming;


        /// <summary>
        /// Конструктор класса, предоставляющего <see cref="IAngleFinder"/>
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param> для чистового отверстия для размещения по одному заданию на отверстие
        /// <exception cref="ArgumentNullException"/>
        public SingleOpeningTaskAngleFinderProvider(OpeningMepTaskIncoming incomingTask) {
            if(incomingTask is null) { throw new ArgumentNullException(nameof(incomingTask)); }

            _openingMepTaskIncoming = incomingTask;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public IAngleFinder GetAngleFinder() {
            switch(_openingMepTaskIncoming.OpeningType) {
                case OpeningType.WallRound:
                case OpeningType.WallRectangle:
                case OpeningType.FloorRound:
                return new ZeroAngleFinder();
                case OpeningType.FloorRectangle:
                return new SingleRectangleOpeningTaskInFloorAngleFinder(_openingMepTaskIncoming);
                default:
                throw new ArgumentException(nameof(_openingMepTaskIncoming.OpeningType));
            }
        }
    }
}
