using System;
using System.Collections.Generic;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.ParameterGetters {
    /// <summary>
    /// Класс, предоставляющий параметры отверстия КР, размещаемого в стене по нескольким заданиям от АР
    /// </summary>
    internal class ManyOpeningArTasksInWallParameterGetter : IParametersGetter {
        private readonly ICollection<OpeningArTaskIncoming> _incomingTasks;
        private readonly IPointFinder _pointFinder;


        /// <summary>
        /// Конструктор класса, предоставляющего параметры отверстия КР, размещаемого в стене по нескольким заданиям от АР
        /// </summary>
        /// <param name="incomingTasks">Входящие задания от АР</param>
        /// <param name="pointFinder">Провайдер точки вставки отверситя КР</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ManyOpeningArTasksInWallParameterGetter(ICollection<OpeningArTaskIncoming> incomingTasks, IPointFinder pointFinder) {
            _incomingTasks = incomingTasks ?? throw new ArgumentNullException(nameof(incomingTasks));
            if(_incomingTasks.Count < 1) { throw new ArgumentException(nameof(incomingTasks)); }

            _pointFinder = pointFinder ?? throw new ArgumentNullException(nameof(pointFinder));
        }


        public IEnumerable<ParameterValuePair> GetParamValues() {
            throw new NotImplementedException();
        }
    }
}
