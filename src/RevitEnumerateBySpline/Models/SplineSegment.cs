using Autodesk.Revit.DB;

namespace RevitEnumerateBySpline.Models;

/// <summary>
/// Отрезок аппроксимации линии в порядке нумерации.
/// </summary>
internal sealed class SplineSegment {
    public SplineSegment(CurveModel curveModel, Line curve, int curveIndex, int segmentIndex, int index) {
        CurveModel = curveModel;
        Curve = curve;
        CurveIndex = curveIndex;
        SegmentIndex = segmentIndex;
        Index = index;
    }

    public CurveModel CurveModel { get; }
    public Line Curve { get; }
    public int CurveIndex { get; }
    public int SegmentIndex { get; }
    public int Index { get; }
}
