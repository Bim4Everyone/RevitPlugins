using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface ISpatialSolidService {
    /// <summary>
    /// Создание солида зоны.
    /// </summary>
    /// <remarks>
    /// В данном методе извлекается внешний контур зоны,
    /// на основе которого формируются замкнутые контуры. Затем контуры
    /// перемещаются по оси Z таким образом, чтобы начальная отметка соответствовала
    /// значению -1000, после чего выполняется их экструзия в диапазоне от -1000 до 1000.
    /// </remarks>
    /// <param name="spatialElement">Зона, для которой создается солид.</param>
    /// <returns>
    /// Солид, сформированный на основе внешнего контура зоны.
    /// </returns>
    Solid CreateSpatialSolid(SpatialElement spatialElement);
}
