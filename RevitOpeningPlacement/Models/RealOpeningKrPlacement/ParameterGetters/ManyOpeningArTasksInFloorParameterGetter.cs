using System;
using System.Collections.Generic;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.ParameterGetters {
    /// <summary>
    /// Класс, предоставляющий параметры отверстия КР, размещаемого в перекрытии по нескольким заданиям от АР
    /// </summary>
    internal class ManyOpeningArTasksInFloorParameterGetter : IParametersGetter {
        private readonly ICollection<OpeningArTaskIncoming> _incomingTasks;


        /// <summary>
        /// Конструктор класса, предоставляющего параметры отверстия КР, размещаемого в перекрытии по нескольким заданиям от АР
        /// </summary>
        /// <param name="incomingTasks">Входящие задания от АР</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ManyOpeningArTasksInFloorParameterGetter(ICollection<OpeningArTaskIncoming> incomingTasks) {
            _incomingTasks = incomingTasks ?? throw new ArgumentNullException(nameof(incomingTasks));
        }


        public IEnumerable<ParameterValuePair> GetParamValues() {
            throw new NotImplementedException();
        }
    }
}
