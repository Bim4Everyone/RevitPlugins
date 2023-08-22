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
    /// Класс, предоставляющий параметры габаритов чистового отверстия, размещаемого в перекрытии по нескольким заданиям на отверстия
    /// </summary>
    internal class ManyOpeningTasksInFloorParameterGetter : IParametersGetter {
        private readonly ICollection<OpeningMepTaskIncoming> _incomingTasks;


        /// <summary>
        /// Конструктор класса, предоставляющего параметров габариты чистового отверстия, размещаемого в перекрытии по нескольким заданиям на отверстия
        /// </summary>
        /// <param name="incomingTasks">Входящие задания на отверстия</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ManyOpeningTasksInFloorParameterGetter(ICollection<OpeningMepTaskIncoming> incomingTasks) {
            _incomingTasks = incomingTasks ?? throw new ArgumentNullException(nameof(incomingTasks));
            if(_incomingTasks.Count < 1) { throw new ArgumentException(nameof(incomingTasks)); }
        }


        public IEnumerable<ParameterValuePair> GetParamValues() {
            var box = GetUnitedBox(_incomingTasks);
            var height = box.Max.Y - box.Min.Y;
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
