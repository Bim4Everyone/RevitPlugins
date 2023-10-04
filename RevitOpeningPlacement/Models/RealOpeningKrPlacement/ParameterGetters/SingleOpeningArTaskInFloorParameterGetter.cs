using System;
using System.Collections.Generic;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.RealOpeningKrPlacement.ValueGetters;
using RevitOpeningPlacement.Models.RealOpeningsGeometryValueGetters;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.ParameterGetters {
    /// <summary>
    /// Класс, предоставляющий значения параметров для КР отверстия, размещаемого по одному заданию на отверстие от АР
    /// </summary>
    internal class SingleOpeningArTaskInFloorParameterGetter : IParametersGetter {
        private readonly OpeningArTaskIncoming _incomingTask;


        /// <summary>
        /// Конструктор класса, предоставляющего значения параметров для КР отверстия, размещаемого по одному заданию на отверстие от АР
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие от АР</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SingleOpeningArTaskInFloorParameterGetter(OpeningArTaskIncoming incomingTask) {
            _incomingTask = incomingTask ?? throw new ArgumentNullException(nameof(incomingTask));
        }


        public IEnumerable<ParameterValuePair> GetParamValues() {
            // габариты отверстия
            yield return new DoubleParameterGetter(
                RealOpeningKrPlacer.RealOpeningKrInFloorHeight,
                new RectangleOpeningInFloorHeightValueGetter(_incomingTask)).GetParamValue();
            yield return new DoubleParameterGetter(
                RealOpeningKrPlacer.RealOpeningKrInFloorWidth,
                new RectangleOpeningInFloorWidthValueGetter(_incomingTask)).GetParamValue();

            // текстовые данные отверстия
            yield return new StringParameterGetter(
                RealOpeningKrPlacer.RealOpeningTaskId,
                new KrTaskIdValueGetter(_incomingTask)).GetParamValue();
        }
    }
}
