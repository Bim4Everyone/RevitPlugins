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
    public void Enumerate(EnumerateBySplineSettings settings) {
        var points = splineDividerService.Divide(settings.CurveModels ?? []);
        var orderedIntersections = intersectionService
            .FindIntersections(settings.SpatialModels ?? [], points)
            .OrderBy(intersection => intersection.SplinePoint.Index)
            .ToList();
        if(settings.ConfigSettings != null) {
            paramSetter.SetParams(orderedIntersections, settings.ConfigSettings);
        }
    }
}
