using System;
using System.Collections.Generic;

namespace RevitPylonLoadAreas.Models.Geometry;

/// <summary>
/// Прямоугольная ограничивающая рамка на плоскости XY.
/// </summary>
internal readonly struct BoundingBox2D {
    public BoundingBox2D(double minX, double minY, double maxX, double maxY) {
        MinX = minX;
        MinY = minY;
        MaxX = maxX;
        MaxY = maxY;
    }

    public double MinX { get; }
    public double MinY { get; }
    public double MaxX { get; }
    public double MaxY { get; }

    public double Width => MaxX - MinX;
    public double Height => MaxY - MinY;
    public double Diagonal => Math.Sqrt(Width * Width + Height * Height);
    public Point2D Center => new((MinX + MaxX) * 0.5, (MinY + MaxY) * 0.5);

    public static BoundingBox2D FromPoints(IEnumerable<Point2D> points) {
        double minX = double.PositiveInfinity, minY = double.PositiveInfinity;
        double maxX = double.NegativeInfinity, maxY = double.NegativeInfinity;
        bool any = false;
        foreach(var p in points) {
            any = true;
            if(p.X < minX) {
                minX = p.X;
            }
            if(p.X > maxX) {
                maxX = p.X;
            }
            if(p.Y < minY) {
                minY = p.Y;
            }
            if(p.Y > maxY) {
                maxY = p.Y;
            }
        }
        if(!any) {
            return new BoundingBox2D(0, 0, 0, 0);
        }
        return new BoundingBox2D(minX, minY, maxX, maxY);
    }

    public BoundingBox2D Expanded(double margin) {
        return new BoundingBox2D(MinX - margin, MinY - margin, MaxX + margin, MaxY + margin);
    }

    public BoundingBox2D Union(BoundingBox2D other) {
        return new BoundingBox2D(
            Math.Min(MinX, other.MinX),
            Math.Min(MinY, other.MinY),
            Math.Max(MaxX, other.MaxX),
            Math.Max(MaxY, other.MaxY));
    }

    public bool Contains(Point2D p) {
        return p.X >= MinX && p.X <= MaxX && p.Y >= MinY && p.Y <= MaxY;
    }
}
