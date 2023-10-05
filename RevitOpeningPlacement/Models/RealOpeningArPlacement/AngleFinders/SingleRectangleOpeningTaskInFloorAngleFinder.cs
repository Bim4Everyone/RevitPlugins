using System;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.AngleFinders {
    /// <summary>
    /// Класс, предоставляющий угол поворота задания на прямоугольное отверстие в перекрытии в горизонтальной плоскости в единицах ревита
    /// </summary>
    internal class SingleRectangleOpeningTaskInFloorAngleFinder : IAngleFinder {
        private readonly OpeningMepTaskIncoming _openingMepTaskIncoming;


        /// <summary>
        /// Конструктор класса, предоставляющего угол поворота задания на прямоугольное отверстие в перекрытии в горизонтальной плоскости в единицах ревита
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SingleRectangleOpeningTaskInFloorAngleFinder(OpeningMepTaskIncoming incomingTask) {
            if(incomingTask is null) { throw new ArgumentNullException(nameof(incomingTask)); }

            _openingMepTaskIncoming = incomingTask;
        }


        public Rotates GetAngle() {
            return new Rotates(0, 0, _openingMepTaskIncoming.Rotation);
        }
    }
}
