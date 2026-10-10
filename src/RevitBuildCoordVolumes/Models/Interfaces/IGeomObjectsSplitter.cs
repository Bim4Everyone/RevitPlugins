using System.Collections.Generic;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Services;

namespace RevitBuildCoordVolumes.Models.Interfaces;
/// <summary>
/// Разбиение геометрических объектов на непересекающиеся части.
/// </summary>
/// <remarks>
/// В данном методе из исходных геометрических объектов извлекаются солиды,
/// которые последовательно разбиваются на непересекающиеся части.
/// Для каждого полученного солида создается новый геометрический объект
/// с указанием уровня и объема.
/// Если после разбиения солиды отсутствуют, возвращается пустой список.
/// </remarks>
/// <param name="geomObjects">Исходный список геометрических объектов.</param>
/// <param name="columnGroupObject">Список объектов колонн, содержащих информацию об уровне.</param>
/// <param name="spatialObject">Пространственный объект, связанный с обрабатываемой геометрией.</param>
/// <param name="progressService">Прогресс-сервис</param>
/// <returns>
/// Список геометрических объектов, сформированных из непересекающихся солидов.
/// </returns>
internal interface IGeomObjectsSplitter {
    List<GeomObject> SplitGeomObjects(
        List<GeomObject> geomObjects, 
        ColumnGroupObject columnGroupObject, 
        SpatialObject spatialObject, 
        ProgressService progressService);
}
