using System;
using System.Collections.Generic;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.RealOpeningKrPlacement.ValueGetters;
using RevitOpeningPlacement.Models.RealOpeningsGeometryValueGetters;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.ParameterGetters;
/// <summary>
/// Класс, предоставляющий значения параметров для КР отверстия, размещаемого по одному заданию на отверстие
/// </summary>
internal class SingleOpeningArTaskInFloorParameterGetter : IParametersGetter {
    private readonly IOpeningTaskIncoming _incomingTask;
    private readonly int _rounding;


    /// <summary>
    /// Конструктор класса, предоставляющего значения параметров для КР отверстия, размещаемого по одному заданию на отверстие
    /// </summary>
    /// <param name="incomingTask">Входящее задание на отверстие</param>
    /// <param name="rounding">Округление размеров отверстия в мм</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public SingleOpeningArTaskInFloorParameterGetter(IOpeningTaskIncoming incomingTask, int rounding) {
        _incomingTask = incomingTask ?? throw new ArgumentNullException(nameof(incomingTask));
        _rounding = rounding;
    }


    public IEnumerable<ParameterValuePair> GetParamValues() {
        // габариты отверстия
        yield return new DoubleParameterGetter(
            RealOpeningKrPlacer.RealOpeningKrInFloorHeight,
            new RectangleOpeningInFloorHeightValueGetter(_incomingTask, _rounding)).GetParamValue();
        yield return new DoubleParameterGetter(
            RealOpeningKrPlacer.RealOpeningKrInFloorWidth,
            new RectangleOpeningInFloorWidthValueGetter(_incomingTask, _rounding)).GetParamValue();

        // текстовые данные отверстия
        yield return new StringParameterGetter(
            RealOpeningKrPlacer.RealOpeningTaskId,
            new KrTaskIdValueGetter(_incomingTask)).GetParamValue();
    }
}
