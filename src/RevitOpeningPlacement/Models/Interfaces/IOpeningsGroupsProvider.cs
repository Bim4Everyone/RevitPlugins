using System.Collections.Generic;

using RevitOpeningPlacement.Models.OpeningUnion;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.Interfaces {
    /// <summary>
    /// Интерфейс, предоставляющий коллекцию групп заданий на отверстия.
    /// </summary>
    internal interface IOpeningsGroupsProvider {
        /// <summary>
        /// Создает коллекцию групп исходящих заданий на отверстия из коллекции исходящих заданий на отверстия
        /// </summary>
        /// <param name="openingTasks">Коллекция исходящих заданий на отверстия из активного документа</param>
        /// <returns></returns>
        ICollection<OpeningsGroup> GetOpeningsGroups(ICollection<OpeningMepTaskOutcoming> openingTasks);
    }
}
