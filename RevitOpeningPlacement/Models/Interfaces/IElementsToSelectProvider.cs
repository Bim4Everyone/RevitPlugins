using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Interfaces {
    /// <summary>
    /// Интерфейс предоставляющий коллекцию элементов, которые надо выделить на виде
    /// </summary>
    internal interface IElementsToSelectProvider {
        /// <summary>
        /// Возвращает коллекцию элементов, которые надо выделить на виде
        /// </summary>
        /// <returns></returns>
        ICollection<Element> GetElementsToSelect();
    }
}
