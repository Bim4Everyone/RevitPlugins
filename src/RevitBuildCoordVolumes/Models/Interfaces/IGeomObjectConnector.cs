using System.Collections.Generic;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Services;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface IGeomObjectConnector {
    /// <summary>
    /// Метод объединения объектов GeomObject.
    /// </summary>
    /// <remarks>
    /// В данном методе объединение объектов GeomObject в единый GeomObject, с 3 попытками перемешивания, в случае неудачи.
    /// </remarks>
    /// <param name="geomObjects">Исходные объекты GeomObject.</param>   
    /// /// <param name="progressService">Прогресс сервис.</param>   
    /// <returns>
    /// Объединенные объекты GeomObject.
    /// </returns>
    List<GeomObject> UnionGeomObjects(List<GeomObject> geomObjects, ProgressService progressService);
}
