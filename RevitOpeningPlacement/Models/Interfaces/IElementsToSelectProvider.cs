using System.Collections.Generic;

using RevitClashDetective.Models.Clashes;

namespace RevitOpeningPlacement.Models.Interfaces {
    /// <summary>
    /// Интерфейс предоставляющий коллекцию элементов, которые надо выделить на виде
    /// </summary>
    internal interface IElementsToSelectProvider {
        /// <summary>
        /// Возвращает коллекцию элементов, которые надо выделить на виде
        /// </summary>
        /// <returns></returns>
        ICollection<ElementModel> GetElementsToSelect();
    }
}
