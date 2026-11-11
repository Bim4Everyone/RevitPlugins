using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitEnumerateBySpline.Models.Interfaces;

namespace RevitEnumerateBySpline.Models.Services;

/// <summary>
/// Разбивает линии на отрезки в порядке, заданном в интерфейсе.
/// </summary>
internal sealed class SplineDividerService : ISplineDividerService {
    private static readonly double _maxSegmentLength =
        UnitUtils.ConvertToInternalUnits(10, UnitTypeId.Millimeters);

    public IReadOnlyList<SplineSegment> Divide(IEnumerable<CurveModel?> curveModels) {
        var segments = new List<SplineSegment>();
        int curveIndex = 0;

        foreach(var curveModel in curveModels) {
            if(curveModel?.CurveElement?.GeometryCurve is not { } curve) {
                curveIndex++;
                continue;
            }

            var points = curve.Tessellate();
            if(points.Count < 2) {
                curveIndex++;
                continue;
            }

            double shortCurveTolerance = curveModel.CurveElement.Document.Application.ShortCurveTolerance;
            int segmentIndex = 0;
            XYZ startPoint = points[0];

            for(int pointIndex = 1; pointIndex < points.Count; pointIndex++) {
                XYZ endPoint = points[pointIndex];
                double edgeLength = startPoint.DistanceTo(endPoint);
                if(edgeLength <= shortCurveTolerance) {
                    continue;
                }

                int partCount = (int) Math.Ceiling(edgeLength / _maxSegmentLength);
                XYZ partStart = startPoint;

                for(int partIndex = 1; partIndex <= partCount; partIndex++) {
                    XYZ partEnd = startPoint + (endPoint - startPoint).Multiply((double) partIndex / partCount);
                    var segmentCurve = Line.CreateBound(partStart, partEnd);
                    segments.Add(new SplineSegment(curveModel, segmentCurve, curveIndex, segmentIndex, segments.Count));
                    segmentIndex++;
                    partStart = partEnd;
                }

                startPoint = endPoint;
            }

            curveIndex++;
        }

        return segments;
    }
}
