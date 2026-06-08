using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitPylonLoadAreas.Models.Geometry;

internal readonly struct BoundingBoxXY {
    public BoundingBoxXY(XY min, XY max) {
        Min = min;
        Max = max;
    }

    public XY Min { get; }
    public XY Max { get; }

    public double Width => Max.X - Min.X;
    public double Height => Max.Y - Min.Y;
    public double Diagonal => Math.Sqrt(Width * Width + Height * Height);
    public XY Center => new((Min.X + Max.X) * 0.5, (Min.Y + Max.Y) * 0.5);

    public static BoundingBoxXY FromPoints(IEnumerable<XY> points) {
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
            return new BoundingBoxXY(new XY(0, 0), new XY(0, 0));
        }
        return new BoundingBoxXY(new XY(minX, minY), new XY(maxX, maxY));
    }

    public static BoundingBoxXY FromBoundingBoxXYZ(BoundingBoxXYZ box) {
        return new BoundingBoxXY(new XY(box.Min.X, box.Min.Y), new XY(box.Max.X, box.Max.Y));
    }

    public BoundingBoxXY Expanded(double margin) {
        return new BoundingBoxXY(
            new XY(Min.X - margin, Min.Y - margin),
            new XY(Max.X + margin, Max.Y + margin));
    }
}
