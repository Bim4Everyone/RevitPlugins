using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitPylonLoadAreas.Models.Geometry;

internal sealed class Polygon2D {
    public Polygon2D(IList<XY> vertices) {
        Vertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
    }

    public IList<XY> Vertices { get; }

    public double SignedArea {
        get {
            if(Vertices.Count < 3) {
                return 0;
            }

            double sum = 0;
            int n = Vertices.Count;
            for(int i = 0; i < n; i++) {
                var a = Vertices[i];
                var b = Vertices[(i + 1) % n];
                sum += (a.X * b.Y) - (b.X * a.Y);
            }

            return sum * 0.5;
        }
    }

    public double Area => Math.Abs(SignedArea);

    public CurveLoop AsCurvLoop(double z) {
        var loop = new CurveLoop();
        int n = Vertices.Count;
        for(int i = 0; i < n; i++) {
            var a = Vertices[i].ToXYZ(z);
            var b = Vertices[(i + 1) % n].ToXYZ(z);
            loop.Append(Line.CreateBound(a, b));
        }

        return loop;
    }

    public bool Contains(XY point) {
        if(Vertices.Count < 3) {
            return false;
        }

        bool inside = false;
        int n = Vertices.Count;
        for(int i = 0, j = n - 1; i < n; j = i++) {
            var pi = Vertices[i];
            var pj = Vertices[j];
            bool crossesY = (pi.Y > point.Y) != (pj.Y > point.Y);
            if(crossesY) {
                double xCross = (pj.X - pi.X) * (point.Y - pi.Y) / (pj.Y - pi.Y) + pi.X;
                if(point.X < xCross) {
                    inside = !inside;
                }
            }
        }

        return inside;
    }

    public Polygon2D EnsureCcw() {
        return SignedArea < 0 ? Reversed() : this;
    }

    public Polygon2D EnsureCw() {
        return SignedArea > 0 ? Reversed() : this;
    }

    private Polygon2D Reversed() {
        var reversed = new List<XY>(Vertices);
        reversed.Reverse();
        return new Polygon2D(reversed);
    }
}
