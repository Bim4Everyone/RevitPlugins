using System;
using System.Collections.Generic;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.RealOpeningPlacement.ValueGetters;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningPlacement.ParameterGetters {
    /// <summary>
    /// Класс для предоставляющий параметры для чистового прямоугольного отверстия из параметров входящего задания на прямоугольное отверстие в стене
    /// </summary>
    internal class SingleRectangleOpeningTaskInWallParameterGetter : IParametersGetter {
        private readonly OpeningMepTaskIncoming _openingMepTaskIncoming;
        private readonly IPointFinder _pointFinder;

        /// <summary>
        /// Конструктор класса, предоставляющего параметры для чистового прямоугольного отверстия из параметров входящего задания на прямоугольное отверстие в стене
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <param name="pointFinder">Провайдер точки вставки чистового отверстия</param>
        public SingleRectangleOpeningTaskInWallParameterGetter(OpeningMepTaskIncoming incomingTask, IPointFinder pointFinder) {
            if(incomingTask is null) { throw new ArgumentNullException(nameof(incomingTask)); }
            if(pointFinder is null) { throw new ArgumentNullException(nameof(pointFinder)); }

            _openingMepTaskIncoming = incomingTask;
            _pointFinder = pointFinder;
        }


        public IEnumerable<ParameterValuePair> GetParamValues() {
            // габариты отверстия
            yield return new DoubleParameterGetter(RealOpeningPlacer.RealOpeningHeight, new RectangleOpeningTaskInWallHeightValueGetter(_openingMepTaskIncoming, _pointFinder)).GetParamValue();
            yield return new DoubleParameterGetter(RealOpeningPlacer.RealOpeningWidth, new RectangleOpeningTaskWidthValueGetter(_openingMepTaskIncoming)).GetParamValue();
        }
    }
}
