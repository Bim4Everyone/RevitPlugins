using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningArPlacement.ParameterGetters;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.Providers {
    /// <summary>
    /// Класс, предоставляющий значения параметров чистового отверстия по нескольким заданиям на отверстия
    /// </summary>
    internal class ManyOpeningTasksParameterGettersProvider {
        private readonly Element _host;
        private readonly ICollection<OpeningMepTaskIncoming> _incomingTasks;
        private readonly IPointFinder _pointFinder;
        private readonly int _rounding;


        /// <summary>
        /// Конструктор класса, предоставляющего значения параметров чистового отверстия по нескольким заданиям на отверстия
        /// </summary>
        /// <param name="host">Хост чистового отверстия</param>
        /// <param name="incomingTasks">Входящие задания на отверстия</param>
        /// <param name="pointFinder">Интерфейс, предоставляющий точку вставки чистового отверстия</param>
        /// <param name="rounding">Округление размеров отверстия в мм</param>
        /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
        /// <exception cref="ArgumentException">Исключение, если в коллекции меньше 1 элемента</exception>
        public ManyOpeningTasksParameterGettersProvider(Element host, ICollection<OpeningMepTaskIncoming> incomingTasks, IPointFinder pointFinder, int rounding) {
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
                return new ManyOpeningTasksInFloorParameterGetter(_incomingTasks, _rounding);
            } else if(_host is Wall wall) {
                return new ManyOpeningTasksInWallParameterGetter(wall, _incomingTasks, _pointFinder, _rounding);
            } else {
                throw new ArgumentException("Тип хоста отверстия не поддерживается");
            }
        }
    }
}
