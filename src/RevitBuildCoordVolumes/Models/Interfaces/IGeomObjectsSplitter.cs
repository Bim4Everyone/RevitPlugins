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
/// <returns>
/// Список геометрических объектов, сформированных из непересекающихся солидов.
/// </returns>
internal interface IGeomObjectsSplitter {
    List<GeomObject> SplitGeomObjects(List<GeomObject> geomObjects, ProgressService progressService);
}
