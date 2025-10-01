using System;
using System.Collections.Generic;
using System.Linq;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.RealOpeningArPlacement.ValueGetters;
using RevitOpeningPlacement.Models.RealOpeningsGeometryValueGetters;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.ParameterGetters;
/// <summary>
/// Класс, предоставляющий параметры габаритов чистового отверстия, размещаемого в перекрытии по нескольким заданиям на отверстия
/// </summary>
internal class ManyOpeningTasksInFloorParameterGetter : IParametersGetter {
    private readonly ICollection<OpeningMepTaskIncoming> _incomingTasks;
    private readonly int _rounding;


    /// <summary>
    /// Конструктор класса, предоставляющего параметров габариты чистового отверстия, размещаемого в перекрытии по нескольким заданиям на отверстия
    /// </summary>
    /// <param name="incomingTasks">Входящие задания на отверстия</param>
    /// <param name="rounding">Округление размеров отверстия в мм</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    /// <exception cref="ArgumentException">Исключение, если в коллекции меньше одного элемента</exception>
    public ManyOpeningTasksInFloorParameterGetter(ICollection<OpeningMepTaskIncoming> incomingTasks, int rounding) {
        _incomingTasks = incomingTasks ?? throw new ArgumentNullException(nameof(incomingTasks));
        _rounding = rounding;
        if(_incomingTasks.Count < 1) { throw new ArgumentException(nameof(incomingTasks)); }
    }


    public IEnumerable<ParameterValuePair> GetParamValues() {
        // габариты отверстия
        yield return new DoubleParameterGetter(
            RealOpeningArPlacer.RealOpeningArHeight,
            new RectangleOpeningInFloorHeightValueGetter(_incomingTasks.Cast<IOpeningTaskIncoming>().ToArray(), _rounding)).GetParamValue();
        yield return new DoubleParameterGetter(
            RealOpeningArPlacer.RealOpeningArWidth,
            new RectangleOpeningInFloorWidthValueGetter(_incomingTasks.Cast<IOpeningTaskIncoming>().ToArray(), _rounding)).GetParamValue();

        // логические флаги для обозначений разделов отверстия
        var isEomValueGetter = new IsEomValueGetter(_incomingTasks);
        var isSsValueGetter = new IsSsValueGetter(_incomingTasks);
        var isOvValueGetter = new IsOvValueGetter(_incomingTasks);
        var isDuValueGetter = new IsDuValueGetter(_incomingTasks);
        var isVkValueGetter = new IsVkValueGetter(_incomingTasks);
        var isTsValueGetter = new IsTsValueGetter(_incomingTasks);
        yield return new IntegerParameterGetter(RealOpeningArPlacer.RealOpeningIsEom, isEomValueGetter).GetParamValue();
        yield return new IntegerParameterGetter(RealOpeningArPlacer.RealOpeningIsSs, isSsValueGetter).GetParamValue();
        yield return new IntegerParameterGetter(RealOpeningArPlacer.RealOpeningIsOv, isOvValueGetter).GetParamValue();
        yield return new IntegerParameterGetter(RealOpeningArPlacer.RealOpeningIsDu, isDuValueGetter).GetParamValue();
        yield return new IntegerParameterGetter(RealOpeningArPlacer.RealOpeningIsVk, isVkValueGetter).GetParamValue();
        yield return new IntegerParameterGetter(RealOpeningArPlacer.RealOpeningIsTs, isTsValueGetter).GetParamValue();
        yield return new IntegerParameterGetter(RealOpeningArPlacer.RealOpeningIsManualBimModelPart, new IsManualBimModelPartValueGetter()).GetParamValue();

        // текстовые данные отверстия
        var manualBimModelPartValueGetter = new ManualBimModelPartValueGetter()
            .SetIsEom(isEomValueGetter)
            .SetIsSs(isSsValueGetter)
            .SetIsOv(isOvValueGetter)
            .SetIsDu(isDuValueGetter)
            .SetIsVk(isVkValueGetter)
            .SetIsTs(isTsValueGetter)
            ;
        yield return new StringParameterGetter(RealOpeningArPlacer.RealOpeningTaskId, new TaskIdValueGetter(_incomingTasks)).GetParamValue();
        yield return new StringParameterGetter(RealOpeningArPlacer.RealOpeningManualBimModelPart, manualBimModelPartValueGetter).GetParamValue();
    }
}
