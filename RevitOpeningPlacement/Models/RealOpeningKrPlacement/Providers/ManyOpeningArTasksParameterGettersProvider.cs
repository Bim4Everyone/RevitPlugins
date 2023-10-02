using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningKrPlacement.ParameterGetters;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.Providers {
    /// <summary>
    /// Класс, предоставляющий провайдеров для параметров размещаемого отверстия КР
    /// </summary>
    internal class ManyOpeningArTasksParameterGettersProvider {
        private readonly Element _host;
        private readonly ICollection<OpeningArTaskIncoming> _incomingTasks;
        private readonly IPointFinder _pointFinder;


        /// <summary>
        /// Конструктор класса, предоставляющего провайдеров для параметров размещаемого отверстия КР
        /// </summary>
        /// <param name="host">Основа для отверстия КР</param>
        /// <param name="incomingTasks">Входящие задания от АР</param>
        /// <param name="pointFinder">Провайдер точки вставки отверстия КР</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ManyOpeningArTasksParameterGettersProvider(Element host, ICollection<OpeningArTaskIncoming> incomingTasks, IPointFinder pointFinder) {
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
                return new ManyOpeningArTasksInFloorParameterGetter(_incomingTasks);
            } else if(_host is Wall wall) {
                return new ManyOpeningArTasksInWallParameterGetter(_incomingTasks, _pointFinder, wall);
            } else {
                throw new ArgumentException(nameof(_host));
            }
        }
    }
}
