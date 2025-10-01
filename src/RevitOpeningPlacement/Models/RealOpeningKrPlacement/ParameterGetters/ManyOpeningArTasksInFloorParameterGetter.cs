using System;
using System.Collections.Generic;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.RealOpeningKrPlacement.ValueGetters;
using RevitOpeningPlacement.Models.RealOpeningsGeometryValueGetters;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.ParameterGetters;
/// <summary>
/// Класс, предоставляющий параметры отверстия КР, размещаемого в перекрытии по нескольким заданиям
/// </summary>
internal class ManyOpeningArTasksInFloorParameterGetter : IParametersGetter {
    private readonly ICollection<IOpeningTaskIncoming> _incomingTasks;
    private readonly int _rounding;


    /// <summary>
    /// Конструктор класса, предоставляющего параметры отверстия КР, размещаемого в перекрытии по нескольким заданиям
    /// </summary>
    /// <param name="incomingTasks">Входящие задания</param>
    /// <param name="rounding">Округление размеров отверстия в мм</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public ManyOpeningArTasksInFloorParameterGetter(ICollection<IOpeningTaskIncoming> incomingTasks, int rounding) {
        _incomingTasks = incomingTasks ?? throw new ArgumentNullException(nameof(incomingTasks));
        _rounding = rounding;
    }


    public IEnumerable<ParameterValuePair> GetParamValues() {
        // габариты отверстия
        yield return new DoubleParameterGetter(
            RealOpeningKrPlacer.RealOpeningKrInFloorHeight,
            new RectangleOpeningInFloorHeightValueGetter(_incomingTasks, _rounding)).GetParamValue();
        yield return new DoubleParameterGetter(
            RealOpeningKrPlacer.RealOpeningKrInFloorWidth,
            new RectangleOpeningInFloorWidthValueGetter(_incomingTasks, _rounding)).GetParamValue();

        // текстовые данные отверстия
        yield return new StringParameterGetter(
            RealOpeningKrPlacer.RealOpeningTaskId,
            new KrTaskIdValueGetter(_incomingTasks)).GetParamValue();
    }
}
