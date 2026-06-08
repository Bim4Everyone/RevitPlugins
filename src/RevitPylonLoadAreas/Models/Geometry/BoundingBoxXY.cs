using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitPylonLoadAreas.Models.Geometry;

internal readonly struct BoundingBoxXY {
    public BoundingBoxXY(XY min, XY max) {
        Min = min;
        Max = max;
    }

    public BoundingBoxXY(IEnumerable<XY> points) {
        if(points == null) {
            throw new ArgumentNullException(nameof(points));
        }

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
            Min = new XY(0, 0);
            Max = new XY(0, 0);
        } else {
            Min = new XY(minX, minY);
            Max = new XY(maxX, maxY);
        }
    }

    public BoundingBoxXY(BoundingBoxXYZ box) {
        if(box == null) {
            throw new ArgumentNullException(nameof(box));
        }

        Min = new XY(box.Min.X, box.Min.Y);
        Max = new XY(box.Max.X, box.Max.Y);
    }

    public XY Min { get; }
    public XY Max { get; }

    public double Width => Max.X - Min.X;
    public double Height => Max.Y - Min.Y;
    public double Diagonal => Math.Sqrt(Width * Width + Height * Height);
    public XY Center => new((Min.X + Max.X) * 0.5, (Min.Y + Max.Y) * 0.5);

    public BoundingBoxXY Expanded(double margin) {
        return new BoundingBoxXY(
            new XY(Min.X - margin, Min.Y - margin),
            new XY(Max.X + margin, Max.Y + margin));
    }
}
