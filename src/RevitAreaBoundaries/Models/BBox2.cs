using System;

using Autodesk.Revit.DB;

namespace RevitAreaBoundaries.Models;

public class BBox2(double minX, double minY, double maxX, double maxY) {
    
    public double MinX { get; } = minX;
    public double MinY { get; } = minY;
    public double MaxX { get; } = maxX;
    public double MaxY { get; } = maxY;

    public bool Intersects(BBox2 other) {
        return !(other.MinX > MaxX || other.MaxX < MinX ||
                 other.MinY > MaxY || other.MaxY < MinY);
    }

    public static BBox2 FromSegmentXY(XYZ a, XYZ b) {
        double minX = Math.Min(a.X, b.X);
        double minY = Math.Min(a.Y, b.Y);
        double maxX = Math.Max(a.X, b.X);
        double maxY = Math.Max(a.Y, b.Y);
        return new BBox2(minX, minY, maxX, maxY);
    }

    public static BBox2 FromCurveXY(Curve c) {
        var pts = c.Tessellate();
        if (pts == null || pts.Count == 0) {
            var p0 = c.GetEndPoint(0);
            var p1 = c.GetEndPoint(1);
            return FromSegmentXY(p0, p1);
        }

        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;

        foreach (var p in pts) {
            if (p.X < minX) minX = p.X;
            if (p.Y < minY) minY = p.Y;
            if (p.X > maxX) maxX = p.X;
            if (p.Y > maxY) maxY = p.Y;
        }

        return new BBox2(minX, minY, maxX, maxY);
    }
}
