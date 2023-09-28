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
            yield return new DoubleParameterGetter(RealOpeningPlacer.RealOpeningArHeight, new RectangleOpeningTaskInFloorHeightValueGetter(_openingMepTaskIncoming)).GetParamValue();
            yield return new DoubleParameterGetter(RealOpeningPlacer.RealOpeningArWidth, new RectangleOpeningTaskWidthValueGetter(_openingMepTaskIncoming)).GetParamValue();

            // логические флаги для обозначений разделов отверстия
            var isEomValueGetter = new IsEomValueGetter(_openingMepTaskIncoming);
            var isSsValueGetter = new IsSsValueGetter(_openingMepTaskIncoming);
            var isOvValueGetter = new IsOvValueGetter(_openingMepTaskIncoming);
            var isDuValueGetter = new IsDuValueGetter(_openingMepTaskIncoming);
            var isVkValueGetter = new IsVkValueGetter(_openingMepTaskIncoming);
            var isTsValueGetter = new IsTsValueGetter(_openingMepTaskIncoming);
            yield return new IntegerParameterGetter(RealOpeningPlacer.RealOpeningIsEom, isEomValueGetter).GetParamValue();
            yield return new IntegerParameterGetter(RealOpeningPlacer.RealOpeningIsSs, isSsValueGetter).GetParamValue();
            yield return new IntegerParameterGetter(RealOpeningPlacer.RealOpeningIsOv, isOvValueGetter).GetParamValue();
            yield return new IntegerParameterGetter(RealOpeningPlacer.RealOpeningIsDu, isDuValueGetter).GetParamValue();
            yield return new IntegerParameterGetter(RealOpeningPlacer.RealOpeningIsVk, isVkValueGetter).GetParamValue();
            yield return new IntegerParameterGetter(RealOpeningPlacer.RealOpeningIsTs, isTsValueGetter).GetParamValue();
            yield return new IntegerParameterGetter(RealOpeningPlacer.RealOpeningIsManualBimModelPart, new IsManualBimModelPartValueGetter()).GetParamValue();

            // текстовые данные отверстия
            var manualBimModelPartValueGetter = new ManualBimModelPartValueGetter()
                .SetIsEom(isEomValueGetter)
                .SetIsSs(isSsValueGetter)
                .SetIsOv(isOvValueGetter)
                .SetIsDu(isDuValueGetter)
                .SetIsVk(isVkValueGetter)
                .SetIsTs(isTsValueGetter)
                ;
            yield return new StringParameterGetter(RealOpeningPlacer.RealOpeningTaskId, new TaskIdValueGetter(_openingMepTaskIncoming)).GetParamValue();
            yield return new StringParameterGetter(RealOpeningPlacer.RealOpeningManualBimModelPart, manualBimModelPartValueGetter).GetParamValue();
        }
    }
}
