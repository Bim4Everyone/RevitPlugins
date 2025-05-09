using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.RealOpeningKrPlacement.ValueGetters;
using RevitOpeningPlacement.Models.RealOpeningsGeometryValueGetters;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.ParameterGetters {
    /// <summary>
    /// Класс, предоставляющий параметры отверстия КР, размещаемого в стене по нескольким заданиям
    /// </summary>
    internal class ManyOpeningArTasksInWallParameterGetter : IParametersGetter {
        private readonly ICollection<IOpeningTaskIncoming> _incomingTasks;
        private readonly IPointFinder _pointFinder;
        private readonly Wall _wall;
        private readonly int _rounding;


        /// <summary>
        /// Конструктор класса, предоставляющего параметры отверстия КР, размещаемого в стене по нескольким заданиям
        /// </summary>
        /// <param name="incomingTasks">Входящие задания</param>
        /// <param name="pointFinder">Провайдер точки вставки отверстия КР</param>
        /// <param name="rounding">Округление размеров отверстия в мм</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        /// <exception cref="ArgumentException">Исключение, если количество элементов в коллекции меньше 1</exception>
        public ManyOpeningArTasksInWallParameterGetter(ICollection<IOpeningTaskIncoming> incomingTasks, IPointFinder pointFinder, Wall wall, int rounding) {
            _incomingTasks = incomingTasks ?? throw new ArgumentNullException(nameof(incomingTasks));
            if(_incomingTasks.Count < 1) { throw new ArgumentException(nameof(incomingTasks)); }

            _pointFinder = pointFinder ?? throw new ArgumentNullException(nameof(pointFinder));
            _wall = wall ?? throw new ArgumentNullException(nameof(wall));
            _rounding = rounding;
        }


        public IEnumerable<ParameterValuePair> GetParamValues() {
            // габариты отверстия
            yield return new DoubleParameterGetter(
                RealOpeningKrPlacer.RealOpeningKrInWallHeight,
                new RectangleOpeningInWallHeightValueGetter(_incomingTasks, _pointFinder, _rounding)).GetParamValue();
            yield return new DoubleParameterGetter(
                RealOpeningKrPlacer.RealOpeningKrInWallWidth,
                new RectangleOpeningInWallWidthValueGetter(_incomingTasks, _wall, _rounding)).GetParamValue();

            // текстовые данные отверстия
            yield return new StringParameterGetter(
                RealOpeningKrPlacer.RealOpeningTaskId,
                new KrTaskIdValueGetter(_incomingTasks)).GetParamValue();
        }
    }
}
