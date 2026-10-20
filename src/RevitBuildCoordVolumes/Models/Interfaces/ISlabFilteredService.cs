using System.Collections.Generic;

using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface ISlabFilteredService {
    /// <summary>
    /// Фильтрация плит по пересечению с пространственным объектом.
    /// </summary>
    /// <remarks>
    /// В данном методе из исходного списка выбираются плиты, хотя бы один проверочный
    /// солид которых пересекается с проверочным солидом пространственного объекта.
    /// </remarks>
    /// <param name="slabs">Список плит для фильтрации.</param>
    /// <param name="spatialObject">Пространственный объект, используемый для проверки пересечений.</param>
    /// <returns>
    /// Список плит, имеющих пересечение с пространственным объектом.
    /// </returns>
    List<SlabElement> FilterSlabBySpatialObject(List<SlabElement> slabs, SpatialObject spatialObject);
}
