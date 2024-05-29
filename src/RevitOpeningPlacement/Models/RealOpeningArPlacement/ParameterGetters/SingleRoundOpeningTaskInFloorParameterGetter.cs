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
    /// Класс, предоставляющий параметры для чистового круглого отверстия из параметров входящего задания на круглое отверстие в перекрытии
    /// </summary>
    internal class SingleRoundOpeningTaskInFloorParameterGetter : IParametersGetter {
        private readonly OpeningMepTaskIncoming _openingMepTaskIncoming;


        /// <summary>
        /// Класс, предоставляющий параметры для чистового круглого отверстия из параметров входящего задания на круглое отверстие в перекрытии
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        public SingleRoundOpeningTaskInFloorParameterGetter(OpeningMepTaskIncoming incomingTask) {
            if(incomingTask is null) { throw new ArgumentNullException(nameof(incomingTask)); }

            _openingMepTaskIncoming = incomingTask;
        }


        public IEnumerable<ParameterValuePair> GetParamValues() {
            // габариты отверстия
            yield return new DoubleParameterGetter(
                RealOpeningArPlacer.RealOpeningArDiameter,
                new RoundOpeningInFloorDiameterValueGetter(_openingMepTaskIncoming)).GetParamValue();

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
