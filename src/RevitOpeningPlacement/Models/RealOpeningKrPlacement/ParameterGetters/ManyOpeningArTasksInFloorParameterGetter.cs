using System;
using System.Collections.Generic;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.RealOpeningKrPlacement.ValueGetters;
using RevitOpeningPlacement.Models.RealOpeningsGeometryValueGetters;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.ParameterGetters {
    /// <summary>
    /// Класс, предоставляющий параметры отверстия КР, размещаемого в перекрытии по нескольким заданиям
    /// </summary>
    internal class ManyOpeningArTasksInFloorParameterGetter : IParametersGetter {
        private readonly ICollection<IOpeningTaskIncoming> _incomingTasks;


        /// <summary>
        /// Конструктор класса, предоставляющего параметры отверстия КР, размещаемого в перекрытии по нескольким заданиям
        /// </summary>
        /// <param name="incomingTasks">Входящие задания</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ManyOpeningArTasksInFloorParameterGetter(ICollection<IOpeningTaskIncoming> incomingTasks) {
            _incomingTasks = incomingTasks ?? throw new ArgumentNullException(nameof(incomingTasks));
        }


        public IEnumerable<ParameterValuePair> GetParamValues() {
            // габариты отверстия
            yield return new DoubleParameterGetter(
                RealOpeningKrPlacer.RealOpeningKrInFloorHeight,
                new RectangleOpeningInFloorHeightValueGetter(_incomingTasks)).GetParamValue();
            yield return new DoubleParameterGetter(
                RealOpeningKrPlacer.RealOpeningKrInFloorWidth,
                new RectangleOpeningInFloorWidthValueGetter(_incomingTasks)).GetParamValue();

            // текстовые данные отверстия
            yield return new StringParameterGetter(
                RealOpeningKrPlacer.RealOpeningTaskId,
                new KrTaskIdValueGetter(_incomingTasks)).GetParamValue();
        }
    }
}
