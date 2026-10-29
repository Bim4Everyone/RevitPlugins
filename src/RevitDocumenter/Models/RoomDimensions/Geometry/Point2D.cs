using System;

namespace RevitDocumenter.Models.RoomDimensions.Geometry;

/// <summary>
/// Точка или вектор на плане. Координаты во внутренних единицах Revit (футы).
/// </summary>
internal readonly struct Point2D {
    public Point2D(double x, double y) {
        X = x;
        Y = y;
    }

    public double X { get; }
    public double Y { get; }

    public double Length => Math.Sqrt(X * X + Y * Y);

    public Point2D Normalize() {
        double length = Length;
        return length < 1e-12 ? new Point2D(0, 0) : new Point2D(X / length, Y / length);
    }

    /// <summary>
    /// Вектор, повернутый на 90 градусов против часовой стрелки.
    /// </summary>
    public Point2D PerpendicularLeft() {
        return new Point2D(-Y, X);
    }

    public double Dot(Point2D other) {
        return X * other.X + Y * other.Y;
    }

    public double Cross(Point2D other) {
        return X * other.Y - Y * other.X;
    }

    public double DistanceTo(Point2D other) {
        return (this - other).Length;
    }

    public static Point2D operator +(Point2D a, Point2D b) => new(a.X + b.X, a.Y + b.Y);
    public static Point2D operator -(Point2D a, Point2D b) => new(a.X - b.X, a.Y - b.Y);
    public static Point2D operator -(Point2D a) => new(-a.X, -a.Y);
    public static Point2D operator *(Point2D a, double k) => new(a.X * k, a.Y * k);

    public override string ToString() {
        return $"({X:0.####}; {Y:0.####})";
    }
}
