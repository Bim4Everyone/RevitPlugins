using System;
using System.Collections.Generic;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.RealOpeningKrPlacement.ValueGetters;
using RevitOpeningPlacement.Models.RealOpeningsGeometryValueGetters;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.ParameterGetters {
    /// <summary>
    /// Класс, предоставляющий параметры для чистового прямоугольного отверстия КР, размещаемого по одному входящему заданию
    /// </summary>
    internal class SingleRectangleOpeningArTaskInWallParameterGetter : IParametersGetter {
        private readonly IOpeningTaskIncoming _incomingTask;
        private readonly IPointFinder _pointFinder;


        /// <summary>
        /// Конструктор класса, предоставляющего параметры для чистового прямоугольного отверстия КР, размещаемого по одному входящему заданию
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <param name="pointFinder">Провайдер точки вставки отверстия КР</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SingleRectangleOpeningArTaskInWallParameterGetter(IOpeningTaskIncoming incomingTask, IPointFinder pointFinder) {
            _incomingTask = incomingTask ?? throw new ArgumentNullException(nameof(incomingTask));
            _pointFinder = pointFinder ?? throw new ArgumentNullException(nameof(pointFinder));
        }


        public IEnumerable<ParameterValuePair> GetParamValues() {
            // габариты отверстия
            yield return new DoubleParameterGetter(
                RealOpeningKrPlacer.RealOpeningKrInWallHeight,
                new RectangleOpeningInWallHeightValueGetter(_incomingTask, _pointFinder)).GetParamValue();
            yield return new DoubleParameterGetter(
                RealOpeningKrPlacer.RealOpeningKrInWallWidth,
                new RectangleOpeningInWallWidthValueGetter(_incomingTask)).GetParamValue();

            // текстовые данные отверстия
            yield return new StringParameterGetter(
                RealOpeningKrPlacer.RealOpeningTaskId,
                new KrTaskIdValueGetter(_incomingTask)).GetParamValue();
        }
    }
}
