using System.Collections.Generic;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models.Interfaces;
internal interface IGeomObjectFactory {
    /// <summary>
    /// Метод получения списка геометрических объектов по списку колонн.
    /// </summary>
    /// <remarks>
    /// В данном методе производится получение разъединенных геометрических объектов для построения DirectShape.
    /// </remarks>
    /// <param name="columnGroups">Колонны, построенные ColumnFactory.</param>   
    /// <param name="polygons">Список всех полигонов зоны.</param>  
    /// <param name="progressService">Прогресс сервис.</param>  
    /// <returns>
    /// Список геометрических элементов GeomObject.
    /// </returns>
    List<GeomObject> CreateIndividualColumnsGeomObjects(
        IList<ColumnGroupObject> columnGroups,
        IList<PolygonObject> polygons,
        ProgressService progressService);
    
    /// <summary>
    /// Метод получения списка геометрических объектов по списку колонн.
    /// </summary>
    /// <remarks>
    /// В данном методе производится получение объединенных геометрических объектов для построения DirectShape.
    /// </remarks>
    /// <param name="columnGroups">Колонны, построенные ColumnFactory.</param>   
    /// <param name="polygons">Список всех полигонов зоны.</param> 
    /// <param name="progressService">Прогресс сервис.</param>  
    /// <returns>
    /// Список геометрических элементов GeomObject.
    /// </returns>
    List<GeomObject> CreateUnitedContourGeomObjects(
        IList<ColumnGroupObject> columnGroups,
        IList<PolygonObject> polygons,
        ProgressService progressService);

    /// <summary>
    /// Получение геометрии контура плиты между начальной и конечной отметками.
    /// </summary>
    /// <remarks>
    /// В данном методе создаются солиды на основе верхних контуров начальной и конечной плит
    /// и выполняется их пересечение. Полученный результат используется для формирования
    /// геометрического объекта с указанием этажа и объема.
    /// Если исходные солиды или результат их пересечения отсутствуют, возвращается пустой список.
    /// </remarks>
    /// <param name="columnGroups">Список объектов колонн, содержащих информацию о начальной и конечной плитах.</param>   
    /// <param name="polygons">Список всех полигонов зоны.</param>
    /// <param name="spatialObject">Обрабатываемая зона</param>
    /// <param name="progressService">Сервис отслеживания прогресса выполнения операции.</param>
    /// <returns>
    /// Список геометрических объектов с контуром плиты и его объемом.
    /// </returns>
    List<GeomObject> CreateSlabContourGeomObjects(
        IList<ColumnGroupObject> columnGroups, 
        IList<PolygonObject> polygons, 
        SpatialObject spatialObject,
        ProgressService progressService);
    
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
    List<GeomObject> CreateSpatialExtrudeGeomObjects(
        BuildCoordVolumeSettings settings,
        SpatialObject spatialObject,
        ProgressService progressService);
}
