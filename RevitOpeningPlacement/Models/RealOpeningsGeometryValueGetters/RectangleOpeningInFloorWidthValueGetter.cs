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


        /// <summary>
        /// Конструктор класса, предоставляющего значение ширины для чистового прямоугольного отверстия АР/КР в перекрытии в единицах ревита
        /// </summary>
        /// <param name="openingTaskIncoming">Входящее задание на отверстие</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RectangleOpeningInFloorWidthValueGetter(IOpeningTaskIncoming openingTaskIncoming) {
            if(openingTaskIncoming is null) { throw new ArgumentNullException(nameof(openingTaskIncoming)); }

            _openingTasksIncoming = new IOpeningTaskIncoming[] { openingTaskIncoming };
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
        }


        public DoubleParamValue GetValue() {
            double widthFeet = GetWidth(_openingTasksIncoming);
            double roundWidthFeet = RoundToCeilingFeetToMillimeters(widthFeet, _widthRound);
            return new DoubleParamValue(roundWidthFeet);
        }


        private double GetWidth(ICollection<IOpeningTaskIncoming> incomingTasks) {
            double width;
            if(incomingTasks.Count == 1) {
                width = GetOpeningWidth(incomingTasks.First());
            } else {
                var box = GetUnitedBox(incomingTasks);
                width = box.Max.X - box.Min.X;
            }
            return width;
        }
    }
}
