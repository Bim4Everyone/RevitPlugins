using System.Collections.Generic;

using RevitEnumerateBySpline.Models.Enums;

namespace RevitEnumerateBySpline.Models.Interfaces;

/// <summary>Предоставляет помещения выбранного диапазона.</summary>
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
