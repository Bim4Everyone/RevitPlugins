using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitPylonLoadAreas.Models.Geometry;

internal sealed class Polygon2D {
    public Polygon2D(IList<XY> vertices) {
        Vertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
    }

    public IList<XY> Vertices { get; }

    public double SignedArea => SignedRingArea(Vertices);

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

    public bool Contains(XY point) => RingContains(Vertices, point);

    public static double SignedRingArea(IList<XY> ring) {
        if(ring == null || ring.Count < 3) {
            return 0;
        }
        double sum = 0;
        int n = ring.Count;
        for(int i = 0; i < n; i++) {
            var a = ring[i];
            var b = ring[(i + 1) % n];
            sum += (a.X * b.Y) - (b.X * a.Y);
        }
        return sum * 0.5;
    }

    public static IList<XY> EnsureCcw(IList<XY> ring) {
        if(SignedRingArea(ring) < 0) {
            return ring.Reverse().ToList();
        }
        return ring;
    }

    public static IList<XY> EnsureCw(IList<XY> ring) {
        if(SignedRingArea(ring) > 0) {
            return ring.Reverse().ToList();
        }
        return ring;
    }

    public static bool RingContains(IList<XY> ring, XY point) {
        if(ring == null || ring.Count < 3) {
            return false;
        }
        bool inside = false;
        int n = ring.Count;
        for(int i = 0, j = n - 1; i < n; j = i++) {
            var pi = ring[i];
            var pj = ring[j];
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
}
