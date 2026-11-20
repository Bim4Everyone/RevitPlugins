namespace RevitEnumerateBySpline.Models;

/// <summary>Первая точка линии, попавшая внутрь помещения.</summary>
internal sealed class SpatialPointIntersection(SpatialModel spatialModel, SplinePoint splinePoint) {
    public SpatialModel SpatialModel { get; } = spatialModel;
    public SplinePoint SplinePoint { get; } = splinePoint;
}
