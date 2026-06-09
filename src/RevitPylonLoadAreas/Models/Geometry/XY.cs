using System;

using Autodesk.Revit.DB;

namespace RevitPylonLoadAreas.Models.Geometry;

/// <summary>
/// Точка на плоскости в координатах Revit
/// </summary>
internal readonly struct XY {
    /// <summary>
    /// Допуск отклонения расстояний на плоскости
    /// </summary>
    public const double Tolerance = 1e-6;

    private readonly XYZ _xyz;

    public XY(double x, double y) {
        _xyz = new XYZ(x, y, 0);
    }

    public XY(XYZ xyz)
        : this(xyz.X, xyz.Y) {
    }

    public double X => _xyz.X;
    public double Y => _xyz.Y;

    public double SqrDistanceTo(XY other) {
        double dx = X - other.X;
        double dy = Y - other.Y;
        return dx * dx + dy * dy;
    }

    public double DistanceTo(XY other) {
        return Math.Sqrt(SqrDistanceTo(other));
    }

    public bool IsAlmostEqual(XY other) {
        return _xyz.IsAlmostEqualTo(other._xyz);
    }

    public XYZ ToXYZ() {
        return new XYZ(X, Y, 0);
    }

    public override string ToString() {
        return $"({X:0.######}, {Y:0.######})";
    }
}
