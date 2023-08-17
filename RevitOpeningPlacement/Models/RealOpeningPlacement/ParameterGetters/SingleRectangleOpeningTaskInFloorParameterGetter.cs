using System;
using System.Collections.Generic;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.RealOpeningPlacement.ValueGetters;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningPlacement.ParameterGetters {
    /// <summary>
    /// Класс для предоставляющий параметры для чистового прямоугольного отверстия из параметров входящего задания на прямоугольное отверстие в перекрытии
    /// </summary>
    internal class SingleRectangleOpeningTaskInFloorParameterGetter : IParametersGetter {
        private readonly OpeningMepTaskIncoming _openingMepTaskIncoming;


        /// <summary>
        /// Конструктор класса, предоставляющего параметры для чистового прямоугольного отверстия из параметров входящего задания на прямоугольное отверстие в перекрытии
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        public SingleRectangleOpeningTaskInFloorParameterGetter(OpeningMepTaskIncoming incomingTask) {
            if(incomingTask is null) { throw new ArgumentNullException(nameof(incomingTask)); }

            _openingMepTaskIncoming = incomingTask;
        }


        public IEnumerable<ParameterValuePair> GetParamValues() {
            // габариты отверстия
            yield return new DoubleParameterGetter(RealOpeningPlacer.RealOpeningHeight, new RectangleOpeningTaskInFloorHeightValueGetter(_openingMepTaskIncoming)).GetParamValue();
            yield return new DoubleParameterGetter(RealOpeningPlacer.RealOpeningWidth, new RectangleOpeningTaskWidthValueGetter(_openingMepTaskIncoming)).GetParamValue();
        }
    }
}
