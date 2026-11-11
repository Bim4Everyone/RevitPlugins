using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitEnumerateBySpline.Models.Enums;

namespace RevitEnumerateBySpline.Models.Interfaces;

internal interface IElementsProvider {
    /// <summary>
    /// Тип источника помещений.
    /// </summary>        
    ElementsProviderType Type { get; }
    /// <summary>
    /// Возвращает помещения выбранного диапазона.
    /// </summary>
    List<SpatialModel> GetSpatialElements();
}
