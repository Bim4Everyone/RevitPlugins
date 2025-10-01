using System;
using System.Collections.Generic;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.RealOpeningKrPlacement.ValueGetters;
using RevitOpeningPlacement.Models.RealOpeningsGeometryValueGetters;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.ParameterGetters;
/// <summary>
/// Класс, предоставляющий параметры для чистового прямоугольного отверстия КР, размещаемого по одному входящему заданию
/// </summary>
internal class SingleRectangleOpeningArTaskInWallParameterGetter : IParametersGetter {
    private readonly IOpeningTaskIncoming _incomingTask;
    private readonly IPointFinder _pointFinder;
    private readonly int _rounding;


    /// <summary>
    /// Конструктор класса, предоставляющего параметры для чистового прямоугольного отверстия КР, размещаемого по одному входящему заданию
    /// </summary>
    /// <param name="incomingTask">Входящее задание на отверстие</param>
    /// <param name="pointFinder">Провайдер точки вставки отверстия КР</param>
    /// <param name="rounding">Округление размеров отверстия в мм</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public SingleRectangleOpeningArTaskInWallParameterGetter(IOpeningTaskIncoming incomingTask, IPointFinder pointFinder, int rounding) {
        _incomingTask = incomingTask ?? throw new ArgumentNullException(nameof(incomingTask));
        _pointFinder = pointFinder ?? throw new ArgumentNullException(nameof(pointFinder));
        _rounding = rounding;
    }


    public IEnumerable<ParameterValuePair> GetParamValues() {
        // габариты отверстия
        yield return new DoubleParameterGetter(
            RealOpeningKrPlacer.RealOpeningKrInWallHeight,
            new RectangleOpeningInWallHeightValueGetter(_incomingTask, _pointFinder, _rounding)).GetParamValue();
        yield return new DoubleParameterGetter(
            RealOpeningKrPlacer.RealOpeningKrInWallWidth,
            new RectangleOpeningInWallWidthValueGetter(_incomingTask, _rounding)).GetParamValue();

        // текстовые данные отверстия
        yield return new StringParameterGetter(
            RealOpeningKrPlacer.RealOpeningTaskId,
            new KrTaskIdValueGetter(_incomingTask)).GetParamValue();
    }
}
