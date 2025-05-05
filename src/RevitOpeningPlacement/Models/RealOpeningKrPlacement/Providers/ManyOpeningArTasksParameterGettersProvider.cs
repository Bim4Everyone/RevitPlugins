using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningKrPlacement.ParameterGetters;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.Providers {
    /// <summary>
    /// Класс, предоставляющий провайдеров для параметров размещаемого отверстия КР
    /// </summary>
    internal class ManyOpeningArTasksParameterGettersProvider {
        private readonly Element _host;
        private readonly ICollection<IOpeningTaskIncoming> _incomingTasks;
        private readonly IPointFinder _pointFinder;
        private readonly int _rounding;


        /// <summary>
        /// Конструктор класса, предоставляющего провайдеров для параметров размещаемого отверстия КР
        /// </summary>
        /// <param name="host">Основа для отверстия КР</param>
        /// <param name="incomingTasks">Входящие задания</param>
        /// <param name="pointFinder">Провайдер точки вставки отверстия КР</param>
        /// <param name="rounding">Округление размеров отверстия в мм</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        /// <exception cref="ArgumentException"><Исключение, если тип хоста не поддерживается, или количество элементов в коллекции меньше 1/exception>
        public ManyOpeningArTasksParameterGettersProvider(Element host, ICollection<IOpeningTaskIncoming> incomingTasks, IPointFinder pointFinder, int rounding) {
            if(host is null) { throw new ArgumentNullException(nameof(host)); }
            if(!((host is Wall) || (host is Floor))) { throw new ArgumentException(nameof(host)); }
            if(incomingTasks is null) { throw new ArgumentNullException(nameof(incomingTasks)); }
            if(incomingTasks.Count < 1) { throw new ArgumentException(nameof(incomingTasks)); }
            if(pointFinder is null) { throw new ArgumentNullException(nameof(pointFinder)); }

            _host = host;
            _incomingTasks = incomingTasks;
            _pointFinder = pointFinder;
            _rounding = rounding;
        }


        public IParametersGetter GetParametersGetter() {
            if(_host is Floor) {
                return new ManyOpeningArTasksInFloorParameterGetter(_incomingTasks, _rounding);
            } else if(_host is Wall wall) {
                return new ManyOpeningArTasksInWallParameterGetter(_incomingTasks, _pointFinder, wall, _rounding);
            } else {
                throw new ArgumentException(nameof(_host));
            }
        }
    }
}
