using System.Collections.Generic;
using System.Linq;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface IGeomObjectsBuildService {
    /// <summary>
    /// Метод генерации GeomObject в зависимости от настроек пользователя.
    /// </summary>
    /// <remarks>
    /// В данном методе производится генерация GeomObject в зависимости от настроек пользователя.
    /// </remarks>
    /// <param name="settings">Настройки, приходящие от пользователя.</param>
    /// <param name="columnGroups">Сгруппированные ColumnObject.</param>
    /// <param name="polygons">Список полигонов, на которые была разбита зона.</param> 
    /// <param name="progressService">ProgressService.</param>
    /// <returns>
    /// Список GeomObject.
    /// </returns>
    List<GeomObject> GetGeomObjects(
        BuildCoordVolumeSettings settings,
        IEnumerable<IGrouping<string, ColumnObject>> columnGroups,
        List<PolygonObject> polygons,
        ProgressService progressService);
}
