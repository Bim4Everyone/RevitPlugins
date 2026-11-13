using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitEnumerateBySpline.Models.Interfaces;

namespace RevitEnumerateBySpline.Models.Services;

/// <summary>
/// Разбивает линии на точки в порядке, заданном в интерфейсе.
/// </summary>
internal sealed class SplineDividerService(SystemPluginConfig systemPluginConfig) : ISplineDividerService {
    private readonly double _maxSegmentLength =
        UnitUtils.ConvertToInternalUnits(systemPluginConfig.MaxSegmentLengthMm, UnitTypeId.Millimeters);

    public IReadOnlyList<SplinePoint> Divide(IEnumerable<CurveModel?> curveModels) {
        var points = new List<SplinePoint>();

        foreach(var curveModel in curveModels) {
            if(curveModel?.CurveElement?.GeometryCurve is not { } curve) {
                continue;
            }

            var tessellatedPoints = curve.Tessellate();
            if(tessellatedPoints.Count < 2) {
                continue;
            }

            var startPoint = tessellatedPoints[0];
            points.Add(new SplinePoint(startPoint, points.Count));

            for(int pointIndex = 1; pointIndex < tessellatedPoints.Count; pointIndex++) {
                var endPoint = tessellatedPoints[pointIndex];
                double edgeLength = startPoint.DistanceTo(endPoint);
                if(edgeLength == 0) {
                    continue;
                }

                int partCount = (int) Math.Ceiling(edgeLength / _maxSegmentLength);

                for(int partIndex = 1; partIndex <= partCount; partIndex++) {
                    var position = startPoint + (endPoint - startPoint).Multiply((double) partIndex / partCount);
                    points.Add(new SplinePoint(position, points.Count));
                }

                startPoint = endPoint;
            }
        }

        return points;
    }
}
