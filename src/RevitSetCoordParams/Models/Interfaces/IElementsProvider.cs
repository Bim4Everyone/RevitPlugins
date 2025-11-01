using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitSetCoordParams.Models.Enums;

namespace RevitSetCoordParams.Models.Interfaces;

internal interface IElementsProvider {
    /// <summary>
    /// Тип провайдера документа
    /// </summary>        
    ElementsProviderType Type { get; }
    /// <summary>
    /// Метод получения элементов модели по категориям
    /// </summary>
    IEnumerable<RevitElement> GetRevitElements(IEnumerable<BuiltInCategory> categories);
}
