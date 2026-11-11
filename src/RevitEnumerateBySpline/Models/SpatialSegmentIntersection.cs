namespace RevitEnumerateBySpline.Models;

/// <summary>
/// Первое пересечение контура помещения с упорядоченными сегментами линий.
/// </summary>
internal sealed class SpatialSegmentIntersection {
    public SpatialSegmentIntersection(SpatialModel spatialModel, SplineSegment splineSegment) {
        SpatialModel = spatialModel;
        SplineSegment = splineSegment;
    }

    public SpatialModel SpatialModel { get; }
    public SplineSegment SplineSegment { get; }
}
