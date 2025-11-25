using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Enums;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface IPositionProvider {
    /// <summary>
    /// Тип провайдера позиции
    /// </summary> 
    PositionProviderType Type { get; }
    /// <summary>
    /// Метод получения координаты элемента по элементу
    /// </summary>
    XYZ GetPositionSlabElement(SlabElement slabElement);
}
