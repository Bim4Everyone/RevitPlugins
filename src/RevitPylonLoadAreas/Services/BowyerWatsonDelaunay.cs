using System;
using System.Collections.Generic;

using RevitPylonLoadAreas.Models.Geometry;
using RevitPylonLoadAreas.Models.Geometry.Delaunay;

namespace RevitPylonLoadAreas.Services;

/// <summary>
/// Триангуляция Делоне
/// </summary>
internal sealed class BowyerWatsonDelaunay {
    private readonly List<XY> _points = new();
    private readonly List<DelaunayTriangle> _triangles = new();

    public IList<DelaunayTriangle> Triangles => _triangles;

    public int[] Triangulate(ICollection<XY> sites) {
        BuildSuperTriangle(sites);
        var indices = new int[sites.Count];
        int i = 0;
        foreach(var site in sites) {
            indices[i] = _points.Count;
            _points.Add(site);
            InsertPoint(indices[i]);
            i++;
        }

        return indices;
    }

    /// <summary>
    /// Строит треугольник, внутри которого находятся все точки
    /// </summary>
    /// <param name="sites">Точки диаграммы Вороного</param>
    private void BuildSuperTriangle(ICollection<XY> sites) {
        // построение равностороннего треугольника, вписанная окружность которого описывает с запасом прямоугольник, ограничивающий все точки
        _points.Clear();
        var bounds = new BoundingBoxXY(sites);
        var center = bounds.GetCenter();
        double r = bounds.GetDiagonalLength() / 2 + 1; // увеличенный на 1 радиус вписанной окружности треугольника
        double t = 6 * r / Math.Sqrt(3); // сторона равностороннего треугольника
        var a = new XY(center.X - t / 2, center.Y - r);
        var b = new XY(center.X, center.Y + 2 * r);
        var c = new XY(center.X + t / 2, center.Y - r);
        _points.Add(a);
        _points.Add(b);
        _points.Add(c);
        _triangles.Add(new DelaunayTriangle(0, 1, 2, _points));
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
