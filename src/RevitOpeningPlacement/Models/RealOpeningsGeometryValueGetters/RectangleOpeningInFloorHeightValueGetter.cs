using System;
using System.Collections.Generic;
using System.Linq;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RealOpeningsGeometryValueGetters {
    /// <summary>
    /// Класс, предоставляющий значение высоты для чистового прямоугольного отверстия АР/КР в перекрытии в единицах ревита
    /// </summary>
    internal class RectangleOpeningInFloorHeightValueGetter : RealOpeningSizeValueGetter, IValueGetter<DoubleParamValue> {
        private readonly ICollection<IOpeningTaskIncoming> _openingTasksIncoming;
        private readonly bool _createdByManyTasks;


        /// <summary>
        /// Конструктор класса, предоставляющего значение высоты для чистового прямоугольного отверстия АР/КР в перекрытии в единицах ревита
        /// </summary>
        /// <param name="openingTaskIncoming">Входящее задание на отверстие</param>
        /// <param name="rounding">Округление размеров отверстия в мм</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        public RectangleOpeningInFloorHeightValueGetter(IOpeningTaskIncoming openingTaskIncoming, int rounding)
            : base(rounding, rounding, rounding) {
            if(openingTaskIncoming is null) { throw new ArgumentNullException(nameof(openingTaskIncoming)); }

            _openingTasksIncoming = new IOpeningTaskIncoming[] { openingTaskIncoming };
            _createdByManyTasks = false;
        }


        /// <summary>
        /// Конструктор класса, предоставляющего значение высоты для чистового прямоугольного отверстия АР/КР в перекрытии в единицах ревита
        /// </summary>
        /// <param name="openingTasksIncoming">Входящие задания на отверстия</param>
        /// <param name="rounding">Округление размеров отверстия в мм</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        /// <exception cref="ArgumentOutOfRangeException">Исключение, если количество элементов в коллекции меньше 1</exception>
        public RectangleOpeningInFloorHeightValueGetter(ICollection<IOpeningTaskIncoming> openingTasksIncoming, int rounding)
            : base(rounding, rounding, rounding) {
            if(openingTasksIncoming == null) { throw new ArgumentNullException(nameof(openingTasksIncoming)); }
            if(openingTasksIncoming.Count < 1) { throw new ArgumentOutOfRangeException(nameof(openingTasksIncoming.Count)); }

            _openingTasksIncoming = openingTasksIncoming;
            _createdByManyTasks = true;
        }


        public DoubleParamValue GetValue() {
            double heightFeet = GetHeight(_openingTasksIncoming, _createdByManyTasks);
            double roundHeightFeet = RoundToCeilingFeetToMillimeters(heightFeet, _heightRound);
            return new DoubleParamValue(roundHeightFeet);
        }

        private double GetHeight(ICollection<IOpeningTaskIncoming> incomingTasks, bool heightByBBox) {
            double height;
            if((incomingTasks.Count == 1) && !heightByBBox) {
                height = GetOpeningHeight(incomingTasks.First());
            } else {
                var box = GetUnitedBox(incomingTasks);
                height = box.Max.Y - box.Min.Y;
            }
            return height;
        }
    }
}
