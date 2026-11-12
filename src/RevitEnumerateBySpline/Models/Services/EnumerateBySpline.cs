using System.Collections.Generic;
using System.Linq;

using RevitEnumerateBySpline.Models.Interfaces;
using RevitEnumerateBySpline.Models.Settings;

namespace RevitEnumerateBySpline.Models.Services;

/// <summary>
/// Управляет подготовкой упорядоченных помещений для нумерации по линиям.
/// </summary>
internal sealed class EnumerateBySpline(
    ISplineDividerService splineDividerService,
    ISplineSpatialIntersectionService intersectionService,
    IParamSetter paramSetter) : IEnumerateBySpline {
    private IReadOnlyList<SpatialPointIntersection> OrderedIntersections { get; set; } = [];

    public void Enumerate(EnumerateBySplineSettings settings) {
        OrderedIntersections = [];
        var points = splineDividerService.Divide(settings.CurveModels ?? []);
        OrderedIntersections = intersectionService
            .FindIntersections(settings.SpatialModels ?? [], points)
            .OrderBy(intersection => intersection.SplinePoint.Index)
            .ToList();
        if(settings.ConfigSettings != null) {
            paramSetter.SetParams(OrderedIntersections, settings.ConfigSettings);
        }
    }
}
