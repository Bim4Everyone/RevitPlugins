using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.ValueGetters {
    /// <summary>
    /// Класс, предоставляющий значение параметра <see cref="RealOpeningArPlacer.RealOpeningTaskId"/>
    /// </summary>
    internal class TaskIdValueGetter : IValueGetter<StringParamValue> {
        private ICollection<OpeningMepTaskIncoming> _incomingMepTasks;


        /// <summary>
        /// Конструктор класса, предоставляющего значение параметра <see cref="RealOpeningArPlacer.RealOpeningTaskId"/>
        /// </summary>
        /// <param name="openingMepTaskIncoming">Входящее задание на отверстие</param>
        public TaskIdValueGetter(OpeningMepTaskIncoming openingMepTaskIncoming) {
            if(openingMepTaskIncoming == null) { throw new ArgumentNullException(nameof(openingMepTaskIncoming)); }

            _incomingMepTasks = new OpeningMepTaskIncoming[] { openingMepTaskIncoming };
        }

        /// <summary>
        /// Конструктор класса, предоставляющего значение параметра <see cref="RealOpeningArPlacer.RealOpeningTaskId"/>
        /// </summary>
        /// <param name="openingsMepTaskIncoming">Коллекция входящих заданий на отверстия</param>
        public TaskIdValueGetter(ICollection<OpeningMepTaskIncoming> openingsMepTaskIncoming) {
            if(openingsMepTaskIncoming == null) { throw new ArgumentNullException(nameof(openingsMepTaskIncoming)); }
            if(openingsMepTaskIncoming.Count < 1) { throw new ArgumentOutOfRangeException(nameof(openingsMepTaskIncoming)); }

            _incomingMepTasks = openingsMepTaskIncoming;
        }


        public StringParamValue GetValue() {
            return new StringParamValue(
                string.Join("; ",
                _incomingMepTasks.GroupBy(opening => Path.GetFileNameWithoutExtension(opening.FileName))
                .Select(group => group.Key + ": " + string.Join("; ", group.Select(opening => opening.Id)))
                ));
        }
    }
}
