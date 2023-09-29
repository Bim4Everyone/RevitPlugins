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


        /// <summary>
        /// Конструктор класса, предоставляющего значения параметров чистового отверстия по нескольким заданиям на отверстия
        /// </summary>
        /// <param name="host">Хост чистового отверстия</param>
        /// <param name="incomingTasks">Входящие задания на отверстия</param>
        /// <param name="pointFinder">Интерфейс, предоставляющий точку вставки чистового отверстия</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ManyOpeningTasksParameterGettersProvider(Element host, ICollection<OpeningMepTaskIncoming> incomingTasks, IPointFinder pointFinder) {
            if(host is null) { throw new ArgumentNullException(nameof(host)); }
            if(!((host is Wall) || (host is Floor))) { throw new ArgumentException(nameof(host)); }
            if(incomingTasks is null) { throw new ArgumentNullException(nameof(incomingTasks)); }
            if(incomingTasks.Count < 1) { throw new ArgumentException(nameof(incomingTasks)); }
            if(pointFinder is null) { throw new ArgumentNullException(nameof(pointFinder)); }

            _host = host;
            _incomingTasks = incomingTasks;
            _pointFinder = pointFinder;
        }


        public IParametersGetter GetParametersGetter() {
            if(_host is Floor) {
                return new ManyOpeningTasksInFloorParameterGetter(_incomingTasks);
            } else if(_host is Wall wall) {
                return new ManyOpeningTasksInWallParameterGetter(wall, _incomingTasks, _pointFinder);
            } else {
                throw new ArgumentException("Тип хоста отверстия не поддерживается");
            }
        }
    }
}
