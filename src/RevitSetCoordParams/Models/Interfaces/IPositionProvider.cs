using Autodesk.Revit.DB;

using RevitSetCoordParams.Models.Enums;

namespace RevitSetCoordParams.Models.Interfaces;

internal interface IPositionProvider {
    /// <summary>
    /// Тип провайдера позиции
    /// </summary> 
    PositionProviderType Type { get; }
    /// <summary>
    /// Метод получения координаты элемента по элементу
    /// </summary>
    XYZ GetPositionElement(RevitElement revitElement);
}
