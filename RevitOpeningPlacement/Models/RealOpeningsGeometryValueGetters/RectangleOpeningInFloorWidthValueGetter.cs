using System;
using System.Collections.Generic;
using System.Linq;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RealOpeningsGeometryValueGetters {
    /// <summary>
    /// Класс, предоставляющий значение ширины для чистового прямоугольного отверстия АР/КР в перекрытии в единицах ревита
    /// </summary>
    internal class RectangleOpeningInFloorWidthValueGetter : RealOpeningSizeValueGetter, IValueGetter<DoubleParamValue> {
        private readonly ICollection<IOpeningTaskIncoming> _openingTasksIncoming;
        private readonly bool _createdByManyTasks;


        /// <summary>
        /// Конструктор класса, предоставляющего значение ширины для чистового прямоугольного отверстия АР/КР в перекрытии в единицах ревита
        /// </summary>
        /// <param name="openingTaskIncoming">Входящее задание на отверстие</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RectangleOpeningInFloorWidthValueGetter(IOpeningTaskIncoming openingTaskIncoming) {
            if(openingTaskIncoming is null) { throw new ArgumentNullException(nameof(openingTaskIncoming)); }

            _openingTasksIncoming = new IOpeningTaskIncoming[] { openingTaskIncoming };
            _createdByManyTasks = false;
        }


        /// <summary>
        /// Конструктор класса, предоставляющего значение ширины для чистового прямоугольного отверстия АР/КР в перекрытии в единицах ревита
        /// </summary>
        /// <param name="openingTasksIncoming">Входящие задания на отверстия</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public RectangleOpeningInFloorWidthValueGetter(ICollection<IOpeningTaskIncoming> openingTasksIncoming) {
            if(openingTasksIncoming == null) { throw new ArgumentNullException(nameof(openingTasksIncoming)); }
            if(openingTasksIncoming.Count < 1) { throw new ArgumentOutOfRangeException(nameof(openingTasksIncoming.Count)); }

            _openingTasksIncoming = openingTasksIncoming;
            _createdByManyTasks = true;
        }


        public DoubleParamValue GetValue() {
            double widthFeet = GetWidth(_openingTasksIncoming, _createdByManyTasks);
            double roundWidthFeet = RoundToCeilingFeetToMillimeters(widthFeet, _widthRound);
            return new DoubleParamValue(roundWidthFeet);
        }


        private double GetWidth(ICollection<IOpeningTaskIncoming> incomingTasks, bool widthByBBox) {
            double width;
            if((incomingTasks.Count == 1) && (!widthByBBox)) {
                width = GetOpeningWidth(incomingTasks.First());
            } else {
                var box = GetUnitedBox(incomingTasks);
                width = box.Max.X - box.Min.X;
            }
            return width;
        }
    }
}
