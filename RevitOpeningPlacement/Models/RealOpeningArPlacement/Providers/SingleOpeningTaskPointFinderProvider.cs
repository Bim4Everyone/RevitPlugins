using System;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningArPlacement.PointFinders;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.Providers {
    /// <summary>
    /// Класс, предоставляющий <see cref="IPointFinder"/> для чистового отверстия для размещения по одному заданию на отверстие
    /// </summary>
    internal class SingleOpeningTaskPointFinderProvider {
        private readonly OpeningMepTaskIncoming _openingMepTaskIncoming;


        /// <summary>
        /// Конструктор класса, предоставляющего <see cref="IPointFinder"/> для чистового отверстия для размещения по одному заданию на отверстие
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SingleOpeningTaskPointFinderProvider(OpeningMepTaskIncoming incomingTask) {
            if(incomingTask is null) { throw new ArgumentNullException(nameof(incomingTask)); }

            _openingMepTaskIncoming = incomingTask;
        }


        public IPointFinder GetPointFinder() {
            return new SingleOpeningTaskPointFinder(_openingMepTaskIncoming);
        }
    }
}
