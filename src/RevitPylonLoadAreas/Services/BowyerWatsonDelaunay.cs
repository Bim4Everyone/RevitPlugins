using System;
using System.Collections.Generic;

using RevitPylonLoadAreas.Models.Geometry;

namespace RevitPylonLoadAreas.Services;

internal sealed class BowyerWatsonDelaunay {
    private readonly List<XY> _points = new();
    private readonly List<Triangle> _triangles = new();

    public (int A, int B, int C) SuperTriangle { get; private set; }

    public IReadOnlyList<XY> Points => _points;
    public IReadOnlyList<Triangle> Triangles => _triangles;

    public int[] Triangulate(IReadOnlyList<XY> sites, BoundingBoxXY bounds, double margin) {
        BuildSuperTriangle(bounds, margin);

        var indices = new int[sites.Count];
        for(int i = 0; i < sites.Count; i++) {
            indices[i] = _points.Count;
            _points.Add(sites[i]);
            InsertPoint(indices[i]);
        }
        return indices;
    }

    public bool IsSuperVertex(int index) {
        return index == SuperTriangle.A || index == SuperTriangle.B || index == SuperTriangle.C;
    }

    private void BuildSuperTriangle(BoundingBoxXY bounds, double margin) {
        var center = bounds.Center;
        double diag = Math.Max(bounds.Diagonal, 1.0) + margin;
        double size = diag * 50;
        var a = new XY(center.X - size, center.Y - size);
        var b = new XY(center.X + size, center.Y - size);
        var c = new XY(center.X, center.Y + size * 2);
        int ia = _points.Count;
        _points.Add(a);
        int ib = _points.Count;
        _points.Add(b);
        int ic = _points.Count;
        _points.Add(c);
        SuperTriangle = (ia, ib, ic);
        _triangles.Add(new Triangle(ia, ib, ic, _points));
    }

    private void InsertPoint(int newIndex) {
        var newPoint = _points[newIndex];
        var bad = new List<int>();
        for(int i = 0; i < _triangles.Count; i++) {
            if(_triangles[i].CircumcircleContains(newPoint)) {
                bad.Add(i);
            }
        }
        if(bad.Count == 0) {
            return;
        }

        var edgeCounts = new Dictionary<EdgeKey, int>();
        foreach(int ti in bad) {
            var t = _triangles[ti];
            CountEdge(edgeCounts, t.V0, t.V1);
            CountEdge(edgeCounts, t.V1, t.V2);
            CountEdge(edgeCounts, t.V2, t.V0);
        }

        var bs = new HashSet<int>(bad);
        for(int i = _triangles.Count - 1; i >= 0; i--) {
            if(bs.Contains(i)) {
                _triangles.RemoveAt(i);
            }
        }

        foreach(var kv in edgeCounts) {
            if(kv.Value == 1) {
                _triangles.Add(new Triangle(kv.Key.A, kv.Key.B, newIndex, _points));
            }
        }
    }

    private static void CountEdge(Dictionary<EdgeKey, int> sink, int a, int b) {
        var key = new EdgeKey(a, b);
        sink.TryGetValue(key, out int c);
        sink[key] = c + 1;
    }

    public readonly struct Triangle {
        public Triangle(int v0, int v1, int v2, IReadOnlyList<XY> points) {
            V0 = v0;
            V1 = v1;
            V2 = v2;
            var a = points[v0];
            var b = points[v1];
            var c = points[v2];
            var center = ComputeCircumcenter(a, b, c);
            Circumcenter = center;
            double dx = center.X - a.X;
            double dy = center.Y - a.Y;
            CircumradiusSquared = dx * dx + dy * dy;
        }

        public int V0 { get; }
        public int V1 { get; }
        public int V2 { get; }
        public XY Circumcenter { get; }
        public double CircumradiusSquared { get; }

        public bool CircumcircleContains(XY p) {
            double dx = p.X - Circumcenter.X;
            double dy = p.Y - Circumcenter.Y;
            return dx * dx + dy * dy <= CircumradiusSquared + GeometryTolerance.Model;
        }

        public static XY ComputeCircumcenter(XY a, XY b, XY c) {
            double ax = a.X, ay = a.Y;
            double bx = b.X, by = b.Y;
            double cx = c.X, cy = c.Y;
            double d = 2.0 * ((ax * (by - cy)) + (bx * (cy - ay)) + (cx * (ay - by)));
            if(Math.Abs(d) < GeometryTolerance.Model) {
                return new XY((ax + bx + cx) / 3.0, (ay + by + cy) / 3.0);
            }
            double ux = (((ax * ax) + (ay * ay)) * (by - cy)
                      + ((bx * bx) + (by * by)) * (cy - ay)
                      + ((cx * cx) + (cy * cy)) * (ay - by)) / d;
            double uy = (((ax * ax) + (ay * ay)) * (cx - bx)
                      + ((bx * bx) + (by * by)) * (ax - cx)
                      + ((cx * cx) + (cy * cy)) * (bx - ax)) / d;
            return new XY(ux, uy);
        }
    }

    private readonly struct EdgeKey : IEquatable<EdgeKey> {
        public EdgeKey(int a, int b) {
            if(a < b) {
                A = a;
                B = b;
            } else {
                A = b;
                B = a;
            }
        }
        public int A { get; }
        public int B { get; }
        public bool Equals(EdgeKey other) => A == other.A && B == other.B;
        public override bool Equals(object obj) => obj is EdgeKey k && Equals(k);
        public override int GetHashCode() => unchecked((A * 397) ^ B);
    }
}
