using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitEnumerateBySpline.Models.Services;

/// <summary>
/// Находит первое пересечение каждого помещения с последовательностью сегментов.
/// </summary>
internal sealed class SplineSpatialIntersectionService(SplineDividerService splineDividerService) {
    private static readonly double _boundingBoxTolerance =
        UnitUtils.ConvertToInternalUnits(10, UnitTypeId.Millimeters);

    public IReadOnlyList<SpatialSegmentIntersection> FindIntersections(
        IEnumerable<SpatialModel?> spatialModels,
        IEnumerable<CurveModel?> curveModels) {

        var segments = splineDividerService.Divide(curveModels);
        var intersections = new List<SpatialSegmentIntersection>();

        foreach(var spatialModel in spatialModels) {
            if(spatialModel?.SpatialElement is not { } spatialElement) {
                continue;
            }

            var boundaryLoops = spatialElement.GetBoundarySegments(new SpatialElementBoundaryOptions());
            if(boundaryLoops is null) {
                continue;
            }

            var boundaryCurves = boundaryLoops
                .SelectMany(loop => loop)
                .Select(boundarySegment => boundarySegment.GetCurve())
                .ToList();
            var boundingBox = spatialElement.get_BoundingBox(null);

            foreach(var segment in segments) {
                if(boundingBox is not null && IsOutsideBoundingBox(segment.Curve, boundingBox)) {
                    continue;
                }

                if(!boundaryCurves.Any(boundaryCurve => Intersects(boundaryCurve, segment.Curve))) {
                    continue;
                }

                intersections.Add(new SpatialSegmentIntersection(spatialModel, segment));
                break;
            }
        }

        // OrderBy сохраняет исходный порядок помещений при совпадении индекса сегмента.
        return intersections.OrderBy(intersection => intersection.SplineSegment.Index).ToList();
    }

    private static bool IsOutsideBoundingBox(Line segment, BoundingBoxXYZ boundingBox) {
        XYZ start = segment.GetEndPoint(0);
        XYZ end = segment.GetEndPoint(1);

        return Math.Max(start.X, end.X) < boundingBox.Min.X - _boundingBoxTolerance
            || Math.Min(start.X, end.X) > boundingBox.Max.X + _boundingBoxTolerance
            || Math.Max(start.Y, end.Y) < boundingBox.Min.Y - _boundingBoxTolerance
            || Math.Min(start.Y, end.Y) > boundingBox.Max.Y + _boundingBoxTolerance;
    }

    private static bool Intersects(Curve boundaryCurve, Curve segmentCurve) {
        var result = boundaryCurve.Intersect(segmentCurve);
        return result is SetComparisonResult.Overlap
            or SetComparisonResult.Subset
            or SetComparisonResult.Superset
            or SetComparisonResult.Equal;
    }
}
