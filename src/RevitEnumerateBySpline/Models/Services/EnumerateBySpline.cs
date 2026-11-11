using System.Collections.Generic;
using System.Linq;

using RevitEnumerateBySpline.Models.Interfaces;
using RevitEnumerateBySpline.Models.Settings;

namespace RevitEnumerateBySpline.Models.Services;

/// <summary>
/// Управляет подготовкой упорядоченных помещений для нумерации по линиям.
/// </summary>
internal sealed class EnumerateBySpline(
    SplineDividerService splineDividerService,
    SplineSpatialIntersectionService intersectionService) : IEnumerateBySpline {

    public IReadOnlyList<SpatialSegmentIntersection> OrderedIntersections { get; private set; } = [];

    public void Enumerate(EnumerateBySplineSettings settings) {
        OrderedIntersections = [];
        var segments = splineDividerService.Divide(settings.CurveModels ?? []);
        OrderedIntersections = intersectionService
            .FindIntersections(settings.SpatialModels ?? [], segments)
            .OrderBy(intersection => intersection.SplineSegment.Index)
            .ToList();
        // Здесь позже будет вызван ParamSetterService для OrderedIntersections.
    }
}
