using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Services;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface ISlabContourSolidService {
    /// <summary>
    /// Создание солида зоны.
    /// </summary>
    /// <remarks>
    /// В данном методе извлекается внешний контур зоны,
    /// на основе которого формируются замкнутые контуры. Затем контуры
    /// перемещаются по оси Z таким образом, чтобы начальная отметка соответствовала
    /// значению -1000, после чего выполняется их экструзия в диапазоне от -1000 до 1000.
    /// </remarks>
    /// <param name="spatialObject">Зона, для которой создается солид.</param>
    /// <returns>
    /// Солид, сформированный на основе внешнего контура зоны.
    /// </returns>
    Solid CreateSpatialSolid(SpatialObject spatialObject);

    Solid CreateFlatSlabContourSolid(
        ColumnObject columnObject,
        Solid spatialSolid,
        ProgressService progressService);
    
    IList<Solid> CreateSlopedSlabContourSolids(
        ColumnObject columnObject, 
        Solid spatialSolid, 
        ProgressService progressService);
}
