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
    /// Класс, предоставляющий параметры для чистового круглого отверстия КР, размещаемого по одному входящему заданию от АР
    /// </summary>
    internal class SingleRoundOpeningArTaskInWallParameterGetter : IParametersGetter {
        private readonly OpeningArTaskIncoming _incomingTask;
        private readonly IPointFinder _pointFinder;


        /// <summary>
        /// Конструктор класса, предоставляющего параметры для чистового круглого отверстия КР, размещаемого по одному входящему заданию от АР
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие от АР</param>
        /// <param name="pointFinder">Провайдер точки вставки отверстия КР</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SingleRoundOpeningArTaskInWallParameterGetter(OpeningArTaskIncoming incomingTask, IPointFinder pointFinder) {
            _incomingTask = incomingTask ?? throw new ArgumentNullException(nameof(incomingTask));
            _pointFinder = pointFinder ?? throw new ArgumentNullException(nameof(pointFinder));
        }


        public IEnumerable<ParameterValuePair> GetParamValues() {
            // габариты отверстия
            yield return new DoubleParameterGetter(
                RealOpeningKrPlacer.RealOpeningKrDiameter,
                new RoundOpeningInWallDiameterValueGetter(_incomingTask, _pointFinder)).GetParamValue();

            // текстовые данные отверстия
            yield return new StringParameterGetter(
                RealOpeningKrPlacer.RealOpeningTaskId,
                new KrTaskIdValueGetter(_incomingTask)).GetParamValue();
        }
    }
}
