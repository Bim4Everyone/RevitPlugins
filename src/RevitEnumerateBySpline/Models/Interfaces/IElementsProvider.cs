using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitEnumerateBySpline.Models.Enums;

namespace RevitEnumerateBySpline.Models.Interfaces;

internal interface IElementsProvider {
    /// <summary>
    /// Тип провайдера элементов
    /// </summary>        
    ElementsProviderType Type { get; }
    /// <summary>
    /// Метод получения элементов модели по стадиям
    /// </summary>
    List<SpatialModel> GetSpatialElements();
}
