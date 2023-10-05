using System;
using System.Collections.Generic;
using System.Linq;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.RealOpeningKrPlacement.ValueGetters;
using RevitOpeningPlacement.Models.RealOpeningsGeometryValueGetters;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.ParameterGetters {
    /// <summary>
    /// Класс, предоставляющий параметры отверстия КР, размещаемого в перекрытии по нескольким заданиям от АР
    /// </summary>
    internal class ManyOpeningArTasksInFloorParameterGetter : IParametersGetter {
        private readonly ICollection<OpeningArTaskIncoming> _incomingTasks;


        /// <summary>
        /// Конструктор класса, предоставляющего параметры отверстия КР, размещаемого в перекрытии по нескольким заданиям от АР
        /// </summary>
        /// <param name="incomingTasks">Входящие задания от АР</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ManyOpeningArTasksInFloorParameterGetter(ICollection<OpeningArTaskIncoming> incomingTasks) {
            _incomingTasks = incomingTasks ?? throw new ArgumentNullException(nameof(incomingTasks));
        }


        public IEnumerable<ParameterValuePair> GetParamValues() {
            // габариты отверстия
            yield return new DoubleParameterGetter(
                RealOpeningKrPlacer.RealOpeningKrInFloorHeight,
                new RectangleOpeningInFloorHeightValueGetter(_incomingTasks.Cast<IOpeningTaskIncoming>().ToArray())).GetParamValue();
            yield return new DoubleParameterGetter(
                RealOpeningKrPlacer.RealOpeningKrInFloorWidth,
                new RectangleOpeningInFloorWidthValueGetter(_incomingTasks.Cast<IOpeningTaskIncoming>().ToArray())).GetParamValue();

            // текстовые данные отверстия
            yield return new StringParameterGetter(
                RealOpeningKrPlacer.RealOpeningTaskId,
                new KrTaskIdValueGetter(_incomingTasks)).GetParamValue();
        }
    }
}
