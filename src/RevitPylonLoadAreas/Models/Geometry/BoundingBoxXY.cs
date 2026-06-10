using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonLoadAreas.Models.Geometry.Voronoi;

namespace RevitPylonLoadAreas.Models.Geometry;

internal readonly struct BoundingBoxXY {
    public BoundingBoxXY(params XY[] points) {
        if(points == null) {
            throw new ArgumentNullException(nameof(points));
        }

        if(points.Length < 2) {
            throw new ArgumentOutOfRangeException(nameof(points));
        }

        double xMin = points.Min(p => p.X);
        double xMax = points.Max(p => p.X);
        double yMin = points.Min(p => p.Y);
        double yMax = points.Max(p => p.Y);
        Min = new XY(xMin, yMin);
        Max = new XY(xMax, yMax);
    }

    public BoundingBoxXY(BoundingBoxXYZ boxXYZ) {
        if(boxXYZ == null) {
            throw new ArgumentNullException(nameof(boxXYZ));
        }

        Min = new XY(boxXYZ.Min);
        Max = new XY(boxXYZ.Max);
    }

    public XY Min { get; }

    public XY Max { get; }

    public double Width => Max.X - Min.X;

    public double Height => Max.Y - Min.Y;

    public double GetDiagonalLength() {
        return Math.Sqrt(Width * Width + Height * Height);
    }

    public XY GetCenter() {
        return new XY((Min.X + Max.X) / 2, (Min.Y + Max.Y) / 2);
    }
}
