using System;
using System.Collections.Generic;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.RealOpeningArPlacement.ValueGetters;
using RevitOpeningPlacement.Models.RealOpeningsGeometryValueGetters;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.ParameterGetters {
    /// <summary>
    /// Класс для предоставляющий параметры для чистового прямоугольного отверстия из параметров входящего задания на прямоугольное отверстие в перекрытии
    /// </summary>
    internal class SingleRectangleOpeningTaskInFloorParameterGetter : IParametersGetter {
        private readonly OpeningMepTaskIncoming _openingMepTaskIncoming;
        private readonly int _rounding;


        /// <summary>
        /// Конструктор класса, предоставляющего параметры для чистового прямоугольного отверстия из параметров входящего задания на прямоугольное отверстие в перекрытии
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <param name="rounding">Округление размеров отверстия в мм</param>
        public SingleRectangleOpeningTaskInFloorParameterGetter(OpeningMepTaskIncoming incomingTask, int rounding) {
            if(incomingTask is null) { throw new ArgumentNullException(nameof(incomingTask)); }

            _openingMepTaskIncoming = incomingTask;
            _rounding = rounding;
        }


        public IEnumerable<ParameterValuePair> GetParamValues() {
            // габариты отверстия
            yield return new DoubleParameterGetter(
                RealOpeningArPlacer.RealOpeningArHeight,
                new RectangleOpeningInFloorHeightValueGetter(_openingMepTaskIncoming, _rounding)).GetParamValue();
            yield return new DoubleParameterGetter(
                RealOpeningArPlacer.RealOpeningArWidth,
                new RectangleOpeningInFloorWidthValueGetter(_openingMepTaskIncoming, _rounding)).GetParamValue();

            // логические флаги для обозначений разделов отверстия
            var isEomValueGetter = new IsEomValueGetter(_openingMepTaskIncoming);
            var isSsValueGetter = new IsSsValueGetter(_openingMepTaskIncoming);
            var isOvValueGetter = new IsOvValueGetter(_openingMepTaskIncoming);
            var isDuValueGetter = new IsDuValueGetter(_openingMepTaskIncoming);
            var isVkValueGetter = new IsVkValueGetter(_openingMepTaskIncoming);
            var isTsValueGetter = new IsTsValueGetter(_openingMepTaskIncoming);
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
            yield return new StringParameterGetter(RealOpeningArPlacer.RealOpeningTaskId, new TaskIdValueGetter(_openingMepTaskIncoming)).GetParamValue();
            yield return new StringParameterGetter(RealOpeningArPlacer.RealOpeningManualBimModelPart, manualBimModelPartValueGetter).GetParamValue();
        }
    }
}
