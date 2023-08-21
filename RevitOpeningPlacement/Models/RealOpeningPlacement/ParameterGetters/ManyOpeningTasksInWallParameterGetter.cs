using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.RealOpeningPlacement.ValueGetters;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningPlacement.ParameterGetters {
    /// <summary>
    /// Класс для предоставляющий параметры для чистового прямоугольного отверстия в стене для нескольких заданий на отверстия
    /// </summary>
    internal class ManyOpeningTasksInWallParameterGetter : IParametersGetter {
        private readonly ICollection<OpeningMepTaskIncoming> _incomingTasks;
        private readonly IPointFinder _pointFinder;


        /// <summary>
        /// Конструктор класса, предоставляющего параметры для чистового прямоугольного отверстия в стене для нескольких заданий на отверстия
        /// </summary>
        /// <param name="incomingTasks">Входящие задания на отверстия</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ManyOpeningTasksInWallParameterGetter(ICollection<OpeningMepTaskIncoming> incomingTasks, IPointFinder pointFinder) {
            _incomingTasks = incomingTasks ?? throw new System.ArgumentNullException(nameof(incomingTasks));
            if(_incomingTasks.Count < 1) { throw new ArgumentException(nameof(incomingTasks)); }
            _pointFinder = pointFinder ?? throw new ArgumentNullException(nameof(pointFinder));
        }


        public IEnumerable<ParameterValuePair> GetParamValues() {
            // используется упрощенный алгоритм для получения ширины и высоты из бокса, т.к. большинство стен в проектах прямолинейные и их оси параллельны OX и OY
            var box = GetUnitedBox(_incomingTasks);
            double zOffset = Math.Abs(box.Min.Z - _pointFinder.GetPoint().Z);
            var height = box.Max.Y - box.Min.Y + zOffset;
            var width = box.Max.X - box.Min.X;

            // габариты отверстия
            yield return new DoubleParameterGetter(RealOpeningPlacer.RealOpeningHeight, new DimensionValueGetter(height)).GetParamValue();
            yield return new DoubleParameterGetter(RealOpeningPlacer.RealOpeningWidth, new DimensionValueGetter(width)).GetParamValue();
        }


        private BoundingBoxXYZ GetUnitedBox(ICollection<OpeningMepTaskIncoming> incomingTasks) {
            return incomingTasks.Select(task => task.GetTransformedBBoxXYZ()).ToList().CreateUnitedBoundingBox();
        }
    }
}
