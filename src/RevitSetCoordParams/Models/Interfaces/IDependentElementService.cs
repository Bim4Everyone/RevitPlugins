using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSetCoordParams.Models.Interfaces;
internal interface IDependentElementService {
    /// <summary>
    /// Карта зависимостей вложенных элементов.
    /// </summary>
    /// <remarks>
    /// В данном свойстве отображается зависимость вложенных элементов от родительского.
    /// </remarks>    
    /// <returns>
    /// Словарь ElementId, где ключ - ID родительского семейства, значение - вложенные семейства.
    /// </returns>
    Dictionary<ElementId, List<ElementId>> DependentMap { get; }
    /// <summary>
    /// Метод, проверяющий, является ли элемент вложенным.
    /// </summary>
    /// <remarks>
    /// В данном методе производится проверка элемента на вложенность или зависимость.
    /// </remarks>       
    /// <param name="element">Элемент.</param>
    /// <returns>
    /// True - если элемент не вложен или независим, False - вложен или зависим.
    /// </returns>
    bool IsRootElement(Element element);
}
