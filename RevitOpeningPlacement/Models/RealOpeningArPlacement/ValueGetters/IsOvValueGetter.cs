﻿using System;
using System.Collections.Generic;
using System.Linq;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.ValueGetters {
    /// <summary>
    /// Класс, предоставляющий значение параметра <see cref="RealOpeningArPlacer.RealOpeningIsOv"/>
    /// </summary>
    internal class IsOvValueGetter : IValueGetter<IntParamValue> {
        private ICollection<OpeningMepTaskIncoming> _incomingMepTasks;


        /// <summary>
        /// Конструктор класса, предоставляющего значение параметра <see cref="RealOpeningArPlacer.RealOpeningIsOv"/>
        /// </summary>
        /// <param name="openingMepTaskIncoming">Входящее задание на отверстие</param>
        public IsOvValueGetter(OpeningMepTaskIncoming openingMepTaskIncoming) {
            if(openingMepTaskIncoming == null) { throw new ArgumentNullException(nameof(openingMepTaskIncoming)); }

            _incomingMepTasks = new OpeningMepTaskIncoming[] { openingMepTaskIncoming };
        }

        /// <summary>
        /// Конструктор класса, предоставляющего значение параметра <see cref="RealOpeningArPlacer.RealOpeningIsOv"/>
        /// </summary>
        /// <param name="openingsMepTaskIncoming">Коллекция входящих заданий на отверстия</param>
        public IsOvValueGetter(ICollection<OpeningMepTaskIncoming> openingsMepTaskIncoming) {
            if(openingsMepTaskIncoming == null) { throw new ArgumentNullException(nameof(openingsMepTaskIncoming)); }
            if(openingsMepTaskIncoming.Count < 1) { throw new ArgumentOutOfRangeException(nameof(openingsMepTaskIncoming)); }

            _incomingMepTasks = openingsMepTaskIncoming;
        }


        public IntParamValue GetValue() {
            return _incomingMepTasks.Any(task =>
            task.FileName.Contains("OV")
            && !task.MepSystem.Contains("Д")
            && (task.MepSystem.Contains("В") || task.MepSystem.Contains("П")))
                ? new IntParamValue(1)
                : new IntParamValue(0);
        }

        public override string ToString() {
            return "ОВ";
        }
    }
}
