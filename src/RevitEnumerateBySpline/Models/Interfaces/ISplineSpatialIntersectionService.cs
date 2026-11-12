using System.Collections.Generic;

namespace RevitEnumerateBySpline.Models.Interfaces;

/// <summary>Определяет попадание точек сплайна в помещения.</summary>
internal interface ISplineSpatialIntersectionService {
    /// <summary>Находит первую точку внутри каждого помещения.</summary>
    /// <param name="spatialModels">Помещения, выбранные для нумерации.</param>
    /// <param name="points">Точки сплайна в порядке следования.</param>
    IReadOnlyList<SpatialPointIntersection> FindIntersections(
        List<SpatialModel?> spatialModels,
        IReadOnlyList<SplinePoint> points);
}
