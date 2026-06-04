using System;

namespace RevitPylonLoadAreas.Models.Geometry;

/// <summary>
/// Точка на плоскости XY (футы Revit).
/// </summary>
internal readonly struct Point2D : IEquatable<Point2D> {
    public Point2D(double x, double y) {
        X = x;
        Y = y;
    }

    public double X { get; }
    public double Y { get; }

    public static Point2D Zero => new(0, 0);

    public Point2D Add(Point2D other) {
        return new Point2D(X + other.X, Y + other.Y);
    }

    public Point2D Sub(Point2D other) {
        return new Point2D(X - other.X, Y - other.Y);
    }

    public Point2D Scale(double s) {
        return new Point2D(X * s, Y * s);
    }

    public double Dot(Point2D other) {
        return X * other.X + Y * other.Y;
    }

    /// <summary>
    /// Z-составляющая векторного произведения (XY) — определитель.
    /// </summary>
    public double Cross(Point2D other) {
        return X * other.Y - Y * other.X;
    }

    public double DistanceTo(Point2D other) {
        double dx = X - other.X;
        double dy = Y - other.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public double SqrDistanceTo(Point2D other) {
        double dx = X - other.X;
        double dy = Y - other.Y;
        return dx * dx + dy * dy;
    }

    /// <summary>
    /// Сравнение с допуском <see cref="GeometryTolerance.Model"/>.
    /// </summary>
    public bool IsAlmostEqual(Point2D other) {
        return IsAlmostEqual(other, GeometryTolerance.Model);
    }

    public bool IsAlmostEqual(Point2D other, double tol) {
        return Math.Abs(X - other.X) <= tol && Math.Abs(Y - other.Y) <= tol;
    }

    public bool Equals(Point2D other) {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object obj) {
        return obj is Point2D p && Equals(p);
    }

    public override int GetHashCode() {
        return unchecked((X.GetHashCode() * 397) ^ Y.GetHashCode());
    }

    public override string ToString() {
        return $"({X:0.######}, {Y:0.######})";
    }
}
