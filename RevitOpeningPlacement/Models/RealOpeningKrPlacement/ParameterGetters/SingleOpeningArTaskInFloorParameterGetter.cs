using System;
using System.Collections.Generic;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.ParameterGetters {
    /// <summary>
    /// Класс, предоставляющий значения параметров для КР отверстия, размещаемого по одному заданию на отверстие от АР
    /// </summary>
    internal class SingleOpeningArTaskInFloorParameterGetter : IParametersGetter {
        private readonly OpeningArTaskIncoming _incomingTask;


        /// <summary>
        /// Конструктор класса, предоставляющего значения параметров для КР отверстия, размещаемого по одному заданию на отверстие от АР
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие от АР</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SingleOpeningArTaskInFloorParameterGetter(OpeningArTaskIncoming incomingTask) {
            _incomingTask = incomingTask ?? throw new ArgumentNullException(nameof(incomingTask));
        }


        public IEnumerable<ParameterValuePair> GetParamValues() {
            throw new NotImplementedException();
        }
    }
}
