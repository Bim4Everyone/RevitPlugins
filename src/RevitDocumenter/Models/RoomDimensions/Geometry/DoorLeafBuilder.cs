using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitDocumenter.Models.RoomDimensions.Geometry;

/// <summary>
/// Распознает полотна по замкнутым четырем линиям символики в группе с дугой открывания (D77).
/// Без надежного контура не подменяет полотно габаритом семейства.
/// </summary>
internal static class DoorLeafBuilder {
    private const double _joinTolerance = 1.0 / 304.8;
    private const double _arcEndTolerance = 50.0 / 304.8;
    private const double _minLeafLength = 200.0 / 304.8;
    private const double _maxThicknessRatio = 0.25;
    private const double _rightAngleTolerance = 0.02;

    public static List<DoorLeafObstacle> Build(long doorId, DoorGeometryData geometry) {
        geometry.ThrowIfNull();
        var result = new List<DoorLeafObstacle>();
        if(geometry.Truncated) {
            return result;
        }
        var groups = geometry.Curves.Where(c => c.Source == "Curve").GroupBy(c => c.GraphicsCategory);
        foreach(var group in groups) {
            var arcs = group.Where(c => c.Kind == "Arc" && c.ModelPointsMm.Count >= 2).ToList();
            if(arcs.Count == 0) {
                continue;
            }
            var nodes = new List<Point2D>();
            var neighbors = new Dictionary<int, HashSet<int>>();
            foreach(var line in group.Where(c => c.Kind == "Line" && c.ModelPointsMm.Count >= 2)) {
                int a = GetNode(nodes, Point(line.ModelPointsMm.First()));
                int b = GetNode(nodes, Point(line.ModelPointsMm.Last()));
                if(a != b) {
                    Connect(neighbors, a, b);
                    Connect(neighbors, b, a);
                }
            }
            var visited = new HashSet<int>();
            foreach(int start in neighbors.Keys) {
                var component = GetComponent(start, neighbors);
                if(!visited.Add(start)) {
                    continue;
                }
                visited.UnionWith(component);
                if(component.Count != 4 || component.Any(i => neighbors[i].Count != 2)) {
                    continue;
                }
                var polygon = OrderRectangle(start, nodes, neighbors);
                if(IsLeaf(polygon, arcs)) {
                    result.Add(new DoorLeafObstacle(doorId, polygon));
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Копирует самостоятельные дуги символики; ребра тела не становятся дугами-препятствиями.
    /// </summary>
    public static List<DoorOpeningArcObstacle> BuildOpeningArcs(long doorId, DoorGeometryData geometry) {
        geometry.ThrowIfNull();
        if(geometry.Truncated) {
            return [];
        }
        return geometry.Curves.Where(c => c.Source == "Curve" && c.Kind == "Arc" && c.ModelPointsMm.Count >= 2)
            .Select(c => new DoorOpeningArcObstacle(doorId, c.ModelPointsMm.Select(Point).ToList()))
            .Where(a => a.Segments.Any(s => s.Length > _joinTolerance)).ToList();
    }

    private static bool IsLeaf(List<Point2D> corners, List<DoorSymbolCurve> arcs) {
        var edges = Enumerable.Range(0, 4).Select(i => corners[(i + 1) % 4] - corners[i]).ToList();
        double length = edges.Max(e => e.Length);
        double thickness = edges.Min(e => e.Length);
        if(length < _minLeafLength || thickness <= _joinTolerance || thickness > length * _maxThicknessRatio) {
            return false;
        }
        for(int i = 0; i < 4; i++) {
            if(Math.Abs(edges[i].Normalize().Dot(edges[(i + 1) % 4].Normalize())) > _rightAngleTolerance) {
                return false;
            }
        }
        return arcs.Any(a => corners.Any(p => p.DistanceTo(Point(a.ModelPointsMm.First())) <= _arcEndTolerance
                                             || p.DistanceTo(Point(a.ModelPointsMm.Last())) <= _arcEndTolerance));
    }

    private static List<Point2D> OrderRectangle(
        int start, List<Point2D> nodes, Dictionary<int, HashSet<int>> neighbors) {
        var result = new List<Point2D> { nodes[start] };
        int previous = -1;
        int current = start;
        for(int i = 0; i < 3; i++) {
            int next = neighbors[current].Where(n => n != previous).Min();
            result.Add(nodes[next]);
            previous = current;
            current = next;
        }
        return result;
    }

    private static HashSet<int> GetComponent(int start, Dictionary<int, HashSet<int>> neighbors) {
        var result = new HashSet<int>();
        var pending = new Stack<int>();
        pending.Push(start);
        while(pending.Count > 0) {
            int current = pending.Pop();
            if(!result.Add(current)) {
                continue;
            }
            foreach(int next in neighbors[current]) {
                pending.Push(next);
            }
        }
        return result;
    }

    private static void Connect(Dictionary<int, HashSet<int>> neighbors, int a, int b) {
        if(!neighbors.TryGetValue(a, out var connected)) {
            connected = [];
            neighbors.Add(a, connected);
        }
        connected.Add(b);
    }

    private static int GetNode(List<Point2D> nodes, Point2D point) {
        int existing = nodes.FindIndex(p => p.DistanceTo(point) <= _joinTolerance);
        if(existing >= 0) {
            return existing;
        }
        nodes.Add(point);
        return nodes.Count - 1;
    }

    private static Point2D Point(double[] mm) {
        return new Point2D(mm[0] / 304.8, mm[1] / 304.8);
    }
}
