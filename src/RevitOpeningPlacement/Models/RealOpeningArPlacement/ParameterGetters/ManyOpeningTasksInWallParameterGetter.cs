using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.RealOpeningArPlacement.ValueGetters;
using RevitOpeningPlacement.Models.RealOpeningsGeometryValueGetters;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.ParameterGetters {
    /// <summary>
    /// Класс для предоставляющий параметры для чистового прямоугольного отверстия в стене для нескольких заданий на отверстия
    /// </summary>
    internal class ManyOpeningTasksInWallParameterGetter : IParametersGetter {
        private readonly Wall _wall;
        private readonly ICollection<OpeningMepTaskIncoming> _incomingTasks;
        private readonly IPointFinder _pointFinder;


        /// <summary>
        /// Конструктор класса, предоставляющего параметры для чистового прямоугольного отверстия в стене для нескольких заданий на отверстия
        /// </summary>
        /// <param name="incomingTasks">Входящие задания на отверстия</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ManyOpeningTasksInWallParameterGetter(Wall wall, ICollection<OpeningMepTaskIncoming> incomingTasks, IPointFinder pointFinder) {
            _wall = wall ?? throw new ArgumentNullException(nameof(wall));
            _incomingTasks = incomingTasks ?? throw new System.ArgumentNullException(nameof(incomingTasks));
            if(_incomingTasks.Count < 1) { throw new ArgumentException(nameof(incomingTasks)); }
            _pointFinder = pointFinder ?? throw new ArgumentNullException(nameof(pointFinder));
        }


        public IEnumerable<ParameterValuePair> GetParamValues() {
            // габариты отверстия
            yield return new DoubleParameterGetter(
                RealOpeningArPlacer.RealOpeningArHeight,
                new RectangleOpeningInWallHeightValueGetter(_incomingTasks.Cast<IOpeningTaskIncoming>().ToArray(), _pointFinder)).GetParamValue();
            yield return new DoubleParameterGetter(
                RealOpeningArPlacer.RealOpeningArWidth,
                new RectangleOpeningInWallWidthValueGetter(_incomingTasks.Cast<IOpeningTaskIncoming>().ToArray(), _wall)).GetParamValue();

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
}
