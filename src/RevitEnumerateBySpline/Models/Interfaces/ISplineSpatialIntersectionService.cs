using System.Collections.Generic;

namespace RevitEnumerateBySpline.Models.Interfaces;

internal interface ISplineSpatialIntersectionService {
    /// <summary>Находит первый пересечённый сегмент для каждого помещения.</summary>
    IReadOnlyList<SpatialSegmentIntersection> FindIntersections(
        List<SpatialModel?> spatialModels,
        IReadOnlyList<SplineSegment> segments);
}
