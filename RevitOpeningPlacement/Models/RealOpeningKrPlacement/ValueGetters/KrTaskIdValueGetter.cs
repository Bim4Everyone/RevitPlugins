using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RealOpeningKrPlacement.ValueGetters {
    /// <summary>
    /// Класс, предоставляющий значение параметра <see cref="RealOpeningKrPlacer.RealOpeningTaskId"/> для отверстия КР
    /// </summary>
    internal class KrTaskIdValueGetter : IValueGetter<StringParamValue> {
        private readonly ICollection<IOpeningTaskIncoming> _incomingTasks;


        /// <summary>
        /// Конструктор класса, предоставляющего значение параметра <see cref="RealOpeningKrPlacer.RealOpeningTaskId"/>
        /// </summary>
        /// <param name="openingArTaskIncoming">Входящее задание на отверстие</param>
        /// <exception cref="ArgumentNullException"></exception>
        public KrTaskIdValueGetter(IOpeningTaskIncoming openingArTaskIncoming) {
            if(openingArTaskIncoming == null) { throw new ArgumentNullException(nameof(openingArTaskIncoming)); }

            _incomingTasks = new IOpeningTaskIncoming[] { openingArTaskIncoming };
        }


        /// <summary>
        /// Конструктор класса, предоставляющего значение параметра <see cref="RealOpeningKrPlacer.RealOpeningTaskId"/>
        /// </summary>
        /// <param name="openingArTaskIncoming">Входящие задания на отверстия</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public KrTaskIdValueGetter(ICollection<IOpeningTaskIncoming> openingArTaskIncoming) {
            if(openingArTaskIncoming == null) { throw new ArgumentNullException(nameof(openingArTaskIncoming)); }
            if(openingArTaskIncoming.Count < 1) { throw new ArgumentOutOfRangeException(nameof(openingArTaskIncoming)); }

            _incomingTasks = openingArTaskIncoming;
        }


        public StringParamValue GetValue() {
            return new StringParamValue(
                string.Join("; ",
                _incomingTasks.GroupBy(opening => Path.GetFileNameWithoutExtension(opening.FileName))
                .Select(group => group.Key + ": " + string.Join("; ", group.Select(opening => opening.Id)))
                ));
        }
    }
}
