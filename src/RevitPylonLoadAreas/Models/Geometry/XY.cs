using System;

using Autodesk.Revit.DB;

namespace RevitPylonLoadAreas.Models.Geometry;

internal readonly struct XY : IEquatable<XY> {
    public XY(double x, double y) {
        X = x;
        Y = y;
    }

    public XY(XYZ xyz) : this(xyz.X, xyz.Y) { }

    public double X { get; }
    public double Y { get; }

    public XY Add(XY other) => new(X + other.X, Y + other.Y);
    public XY Sub(XY other) => new(X - other.X, Y - other.Y);
    public XY Scale(double s) => new(X * s, Y * s);
    public double Dot(XY other) => X * other.X + Y * other.Y;
    public double Cross(XY other) => X * other.Y - Y * other.X;

    public double DistanceTo(XY other) {
        double dx = X - other.X;
        double dy = Y - other.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public double SqrDistanceTo(XY other) {
        double dx = X - other.X;
        double dy = Y - other.Y;
        return dx * dx + dy * dy;
    }

    public bool IsAlmostEqual(XY other) => IsAlmostEqual(other, GeometryTolerance.Model);

    public bool IsAlmostEqual(XY other, double tol) =>
        Math.Abs(X - other.X) <= tol && Math.Abs(Y - other.Y) <= tol;

    public XYZ ToXYZ(double z) => new(X, Y, z);

    public bool Equals(XY other) => X == other.X && Y == other.Y;
    public override bool Equals(object obj) => obj is XY p && Equals(p);
    public override int GetHashCode() => unchecked((X.GetHashCode() * 397) ^ Y.GetHashCode());
    public override string ToString() => $"({X:0.######}, {Y:0.######})";
}
