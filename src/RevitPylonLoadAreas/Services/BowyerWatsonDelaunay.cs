using System;
using System.Collections.Generic;

using RevitPylonLoadAreas.Models.Geometry;

namespace RevitPylonLoadAreas.Services;

internal sealed class BowyerWatsonDelaunay {
    private readonly List<XY> _points = new();
    private readonly List<DelaunayTriangle> _triangles = new();

    public (int A, int B, int C) SuperTriangle { get; private set; }

    public IReadOnlyList<XY> Points => _points;

    public IReadOnlyList<DelaunayTriangle> Triangles => _triangles;

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
        _triangles.Add(new DelaunayTriangle(ia, ib, ic, _points));
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

        var edgeCounts = new Dictionary<DelaunayEdgeKey, int>();
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
                _triangles.Add(new DelaunayTriangle(kv.Key.A, kv.Key.B, newIndex, _points));
            }
        }
    }

    private void CountEdge(Dictionary<DelaunayEdgeKey, int> sink, int a, int b) {
        var key = new DelaunayEdgeKey(a, b);
        sink.TryGetValue(key, out int c);
        sink[key] = c + 1;
    }
}
