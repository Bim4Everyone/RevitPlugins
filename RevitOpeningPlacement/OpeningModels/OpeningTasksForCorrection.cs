using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RevitOpeningPlacement.OpeningModels {
    /// <summary>
    /// Класс, обозначающий коллекцию <see cref="OpeningTaskIncoming">входящих заданий на отверстия</see>, 
    /// к которым есть замечания от получателя, для последующей отправки этой коллекции отправителю заданий
    /// </summary>
    internal class OpeningTasksForCorrection {
        /// <summary>
        /// Создает экземпляр <see cref="OpeningTasksForCorrection"/>
        /// </summary>
        /// <param name="incomingTasks">Коллекция заданий на отверстия с одинаковым файлом источника, 
        /// с имеющимися <seealso cref="OpeningTaskIncoming.Comment">комментариями</seealso></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public OpeningTasksForCorrection(ICollection<OpeningTaskIncoming> incomingTasks) {
            Validate(incomingTasks);

            TaskFile = incomingTasks.First().FileName;
            Tasks = new ReadOnlyCollection<OpeningTaskIncoming>(incomingTasks as IList<OpeningTaskIncoming>);
        }


        /// <summary>
        /// Коллекция заданий на отверстия, к которым есть замечания (комментарии)
        /// </summary>
        public IReadOnlyCollection<OpeningTaskIncoming> Tasks { get; }

        /// <summary>
        /// Название файла-источника заданий на отверстия
        /// </summary>
        public string TaskFile { get; }


        /// <summary>
        /// Проверяет, действительно ли у всех элементов коллекции заданий на отверстия одинаковый файл источника
        /// </summary>
        /// <param name="incomingTasks"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void Validate(ICollection<OpeningTaskIncoming> incomingTasks) {
            var tasksCount = incomingTasks.Count;
            if(tasksCount < 1) {
                throw new ArgumentOutOfRangeException($"Коллекция {nameof(incomingTasks)} пустая. Необходим хотя бы 1 элемент");
            }
            bool filesAreEqual = incomingTasks.Select(t => t.FileName).Distinct().Count() == 1;
            if(!filesAreEqual) {
                throw new ArgumentException($"В коллекции {nameof(incomingTasks)} находятся задания на отверстия из разных файлов");
            }
            if(incomingTasks.Where(t => !string.IsNullOrWhiteSpace(t.Comment)).Count() != tasksCount) {
                throw new ArgumentException($"В коллекции {nameof(incomingTasks)} присутствуют задания на отверстия без комментариев");
            }
        }
    }
}
