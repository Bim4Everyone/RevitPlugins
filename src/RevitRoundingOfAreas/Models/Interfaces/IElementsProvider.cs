using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitRoundingOfAreas.Models.Enums;

namespace RevitRoundingOfAreas.Models.Interfaces;

internal interface IElementsProvider {
    /// <summary>
    /// Тип провайдера элементов
    /// </summary>        
    ElementsProviderType Type { get; }
    /// <summary>
    /// Метод получения элементов модели по стадиям
    /// </summary>
    List<SpatialElement> GetSpatialElements(ElementId phaseId);
}
