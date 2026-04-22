using System.Collections.Generic;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface IGeomObjectFactory {
    /// <summary>
    /// Метод получения списка геометрических объектов по исходной зоне.
    /// </summary>
    /// <remarks>
    /// В данном методе производится получение геометрических объектов для построения DirectShape по исходной зоне.
    /// </remarks>
    /// <param name="settings">Настройки пользователя.</param> 
    /// <param name="spatialObject">Исходная зона.</param> 
    /// <param name="progressService">Прогресс сервис.</param>   
    /// <returns>
    /// Список геометрических элементов GeomObject.
    /// </returns>
    List<GeomObject> GetSimpleGeomObjects(
        BuildCoordVolumeSettings settings,
        SpatialObject spatialObject,
        ProgressService progressService);
    /// <summary>
    /// Метод получения списка геометрических объектов по списку колонн.
    /// </summary>
    /// <remarks>
    /// В данном методе производится получение объединенных геометрических объектов для построения DirectShape.
    /// </remarks>
    /// <param name="columns">Колонны, построенные ColumnFactory.</param>   
    /// <param name="polygonObject">Список всех полигонов зоны.</param> 
    /// <param name="progressService">Прогресс сервис.</param>  
    /// <returns>
    /// Список геометрических элементов GeomObject.
    /// </returns>
    List<GeomObject> GetUnitedGeomObjects(
        List<ColumnObject> columns,
        List<PolygonObject> polygonObject,
        ProgressService progressService);
    /// <summary>
    /// Метод получения списка геометрических объектов по списку колонн.
    /// </summary>
    /// <remarks>
    /// В данном методе производится получение разъединенных геометрических объектов для построения DirectShape.
    /// </remarks>
    /// <param name="columns">Колонны, построенные ColumnFactory.</param>   
    /// <param name="polygonObject">Список всех полигонов зоны.</param>  
    /// <param name="progressService">Прогресс сервис.</param>  
    /// <returns>
    /// Список геометрических элементов GeomObject.
    /// </returns>
    List<GeomObject> GetSeparatedGeomObjects(
        List<ColumnObject> columns,
        List<PolygonObject> polygonObject,
        ProgressService progressService);
}
