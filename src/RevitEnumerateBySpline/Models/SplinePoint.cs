using Autodesk.Revit.DB;

namespace RevitEnumerateBySpline.Models;

/// <summary>Точка линии в порядке нумерации.</summary>
internal sealed class SplinePoint(XYZ position, int index) {
    public XYZ Position { get; } = position;
    public int Index { get; } = index;
}
