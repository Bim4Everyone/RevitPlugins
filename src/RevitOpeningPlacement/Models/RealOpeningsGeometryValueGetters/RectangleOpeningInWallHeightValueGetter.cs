using System;
using System.Collections.Generic;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RealOpeningsGeometryValueGetters {
    /// <summary>
    /// Класс, предоставляющий значение высоты чистового прямоугольного отверстия АР/КР в стене в единицах Revit
    /// </summary>
    internal class RectangleOpeningInWallHeightValueGetter : RealOpeningSizeValueGetter, IValueGetter<DoubleParamValue> {
        private readonly ICollection<IOpeningTaskIncoming> _incomingTasks;
        private readonly IPointFinder _pointFinder;


        /// <summary>
        /// Конструктор класса, предоставляющего значение высоты чистового прямоугольного отверстия АР/КР в стене в единицах Revit
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <param name="pointFinder">Провайдер точки вставки чистового отверстия АР/КР</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RectangleOpeningInWallHeightValueGetter(IOpeningTaskIncoming incomingTask, IPointFinder pointFinder) {
            if(incomingTask == null) { throw new ArgumentNullException(nameof(incomingTask)); }
            _pointFinder = pointFinder ?? throw new ArgumentNullException(nameof(pointFinder));
            _incomingTasks = new IOpeningTaskIncoming[] { incomingTask };
        }

        /// <summary>
        /// Конструктор класса, предоставляющего значение высоты чистового прямоугольного отверстия АР/КР в стене в единицах Revit
        /// </summary>
        /// <param name="incomingTasks">Входящие задания на отверстия</param>
        /// <param name="pointFinder">Провайдер точки вставки чистового отверстия АР/КР</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public RectangleOpeningInWallHeightValueGetter(ICollection<IOpeningTaskIncoming> incomingTasks, IPointFinder pointFinder) {
            if(incomingTasks is null) { throw new ArgumentNullException(nameof(incomingTasks)); }
            if(incomingTasks.Count < 1) { throw new ArgumentOutOfRangeException(nameof(incomingTasks)); }
            _pointFinder = pointFinder ?? throw new ArgumentNullException(nameof(pointFinder));
            _incomingTasks = incomingTasks;
        }


        public DoubleParamValue GetValue() {
            double heightFeet = GetHeight(_incomingTasks, _pointFinder);
            double roundHeightFeet = RoundToCeilingFeetToMillimeters(heightFeet, _heightRound);
            return new DoubleParamValue(roundHeightFeet);
        }


        private double GetHeight(ICollection<IOpeningTaskIncoming> incomingTasks, IPointFinder pointFinder) {
            var box = GetUnitedBox(incomingTasks);
            double zOffset = Math.Abs(box.Min.Z - pointFinder.GetPoint().Z);
            double height = box.Max.Z - box.Min.Z + zOffset;
            return height;
        }
    }
}
