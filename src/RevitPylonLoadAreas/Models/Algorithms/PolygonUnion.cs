using System;
using System.Collections.Generic;
using System.Linq;

using RevitPylonLoadAreas.Models.Geometry;

namespace RevitPylonLoadAreas.Models.Algorithms;

/// <summary>
/// Объединение нескольких 2D-полигонов в результирующий мульти-полигон
/// с помощью графа направленных ребер с попарным сокращением противоположных копий.
///
/// Алгоритм корректно работает для плит, которые либо не пересекаются,
/// либо смыкаются по общим ребрам (типичный паттерн "одна плита смоделирована несколькими элементами").
/// Для случая, когда плиты по-настоящему перекрываются, корректность не гарантируется —
/// это сознательное ограничение v1, поскольку реальные модели описывают именно стыковки.
/// </summary>
internal static class PolygonUnion {
    /// <summary>
    /// Объединяет полигоны. Возвращает список полигонов с дырами.
    /// </summary>
    public static List<Polygon2D> Union(IReadOnlyList<Polygon2D> polygons) {
        if(polygons == null || polygons.Count == 0) {
            return new List<Polygon2D>();
        }
        if(polygons.Count == 1) {
            return new List<Polygon2D> { polygons[0] };
        }

        var quantizer = new VertexQuantizer(GeometryTolerance.Model);

        // Шаг 1. Собираем все направленные ребра (внешние — CCW, дыры — CW).
        var edges = new List<DirectedEdge>();
        foreach(var p in polygons) {
            AppendRing(edges, Polygon2D.EnsureCcw(p.OuterRing), quantizer);
            foreach(var hole in p.Holes) {
                AppendRing(edges, Polygon2D.EnsureCw(hole), quantizer);
            }
        }

        // Шаг 2. Сокращаем противоположные пары: ребро A→B сокращается с ребром B→A.
        var counts = new Dictionary<EdgeKey, int>();
        foreach(var e in edges) {
            var fwd = new EdgeKey(e.From, e.To);
            var rev = new EdgeKey(e.To, e.From);
            if(counts.TryGetValue(rev, out int revCount) && revCount > 0) {
                counts[rev] = revCount - 1;
            } else {
                counts.TryGetValue(fwd, out int c);
                counts[fwd] = c + 1;
            }
        }

        var survivors = new List<DirectedEdge>();
        foreach(var kv in counts) {
            for(int i = 0; i < kv.Value; i++) {
                survivors.Add(new DirectedEdge(kv.Key.From, kv.Key.To));
            }
        }

        if(survivors.Count == 0) {
            return new List<Polygon2D>();
        }

        // Шаг 3. Соединяем оставшиеся ребра в замкнутые контуры.
        var loops = TraceLoops(survivors, quantizer);

        // Шаг 4. Раскладываем контуры на полигоны с дырами по знаку площади и вложенности.
        return AssembleWithHoles(loops, quantizer);
    }

    private static void AppendRing(
        List<DirectedEdge> sink, IReadOnlyList<Point2D> ring, VertexQuantizer q) {
        int n = ring.Count;
        for(int i = 0; i < n; i++) {
            int from = q.Index(ring[i]);
            int to = q.Index(ring[(i + 1) % n]);
            if(from == to) {
                continue;
            }
            sink.Add(new DirectedEdge(from, to));
        }
    }

    private static List<List<Point2D>> TraceLoops(List<DirectedEdge> edges, VertexQuantizer q) {
        var byFrom = new Dictionary<int, List<DirectedEdge>>();
        foreach(var e in edges) {
            if(!byFrom.TryGetValue(e.From, out var list)) {
                list = new List<DirectedEdge>();
                byFrom[e.From] = list;
            }
            list.Add(e);
        }

        var remaining = new HashSet<DirectedEdge>(edges);
        var loops = new List<List<Point2D>>();

        while(remaining.Count > 0) {
            var start = remaining.First();
            remaining.Remove(start);
            var loopIndices = new List<int> { start.From, start.To };
            int current = start.To;
            int loopStart = start.From;
            bool closed = false;

            while(current != loopStart) {
                if(!byFrom.TryGetValue(current, out var outgoing)) {
                    break;
                }
                DirectedEdge? next = null;
                Point2D prev = q.Point(loopIndices[loopIndices.Count - 2]);
                Point2D curr = q.Point(current);
                Point2D inDir = curr.Sub(prev);
                double bestTurn = double.NegativeInfinity;
                foreach(var candidate in outgoing) {
                    if(!remaining.Contains(candidate)) {
                        continue;
                    }
                    Point2D outDir = q.Point(candidate.To).Sub(curr);
                    double turn = LeftTurn(inDir, outDir);
                    if(turn > bestTurn) {
                        bestTurn = turn;
                        next = candidate;
                    }
                }
                if(next == null) {
                    break;
                }
                remaining.Remove(next.Value);
                current = next.Value.To;
                if(current == loopStart) {
                    closed = true;
                    break;
                }
                loopIndices.Add(current);
            }
            if(!closed && current != loopStart) {
                continue;
            }

            var ring = loopIndices.Select(q.Point).ToList();
            ring = RemoveCollinear(ring);
            if(ring.Count >= 3) {
                loops.Add(ring);
            }
        }
        return loops;
    }

    /// <summary>
    /// Чем больше значение, тем "левее" поворот — берем самый левый из доступных,
    /// чтобы корректно обходить контур слева (правило левой руки для CCW-обхода).
    /// </summary>
    private static double LeftTurn(Point2D inDir, Point2D outDir) {
        double cross = inDir.Cross(outDir);
        double dot = inDir.Dot(outDir);
        return Math.Atan2(cross, dot);
    }

    private static List<Point2D> RemoveCollinear(List<Point2D> ring) {
        if(ring.Count < 3) {
            return ring;
        }
        var cleaned = new List<Point2D>(ring.Count);
        int n = ring.Count;
        for(int i = 0; i < n; i++) {
            var prev = ring[(i - 1 + n) % n];
            var cur = ring[i];
            var next = ring[(i + 1) % n];
            var a = cur.Sub(prev);
            var b = next.Sub(cur);
            if(Math.Abs(a.Cross(b)) > GeometryTolerance.Model) {
                cleaned.Add(cur);
            }
        }
        return cleaned;
    }

    private static List<Polygon2D> AssembleWithHoles(List<List<Point2D>> loops, VertexQuantizer q) {
        var outers = new List<List<Point2D>>();
        var holes = new List<List<Point2D>>();
        foreach(var loop in loops) {
            if(Polygon2D.SignedRingArea(loop) > 0) {
                outers.Add(loop);
            } else {
                holes.Add(loop);
            }
        }

        var holeOwners = new Dictionary<int, List<IReadOnlyList<Point2D>>>();
        for(int i = 0; i < outers.Count; i++) {
            holeOwners[i] = new List<IReadOnlyList<Point2D>>();
        }

        foreach(var hole in holes) {
            var probe = hole[0];
            int owner = -1;
            double smallestArea = double.PositiveInfinity;
            for(int i = 0; i < outers.Count; i++) {
                if(!Polygon2D.RingContains(outers[i], probe)) {
                    continue;
                }
                double area = Math.Abs(Polygon2D.SignedRingArea(outers[i]));
                if(area < smallestArea) {
                    smallestArea = area;
                    owner = i;
                }
            }
            if(owner >= 0) {
                holeOwners[owner].Add(hole);
            }
        }

        var result = new List<Polygon2D>();
        for(int i = 0; i < outers.Count; i++) {
            result.Add(new Polygon2D(outers[i], holeOwners[i]));
        }
        return result;
    }

    private readonly struct DirectedEdge : IEquatable<DirectedEdge> {
        public DirectedEdge(int from, int to) {
            From = from;
            To = to;
        }
        public int From { get; }
        public int To { get; }
        public bool Equals(DirectedEdge other) => From == other.From && To == other.To;
        public override bool Equals(object obj) => obj is DirectedEdge e && Equals(e);
        public override int GetHashCode() => unchecked((From * 397) ^ To);
    }

    private readonly struct EdgeKey : IEquatable<EdgeKey> {
        public EdgeKey(int from, int to) {
            From = from;
            To = to;
        }
        public int From { get; }
        public int To { get; }
        public bool Equals(EdgeKey other) => From == other.From && To == other.To;
        public override bool Equals(object obj) => obj is EdgeKey k && Equals(k);
        public override int GetHashCode() => unchecked((From * 397) ^ To);
    }

    /// <summary>
    /// Преобразует <see cref="Point2D"/> в стабильный int-индекс с квантизацией координат
    /// в кратные допуску ячейки. Это устраняет дрейф плавающих чисел в графе ребер.
    /// </summary>
    private sealed class VertexQuantizer {
        private readonly double _tol;
        private readonly Dictionary<(long, long), int> _map = new();
        private readonly List<Point2D> _points = new();

        public VertexQuantizer(double tolerance) {
            _tol = tolerance;
        }

        public int Index(Point2D p) {
            var key = ((long) Math.Round(p.X / _tol), (long) Math.Round(p.Y / _tol));
            if(_map.TryGetValue(key, out int idx)) {
                return idx;
            }
            idx = _points.Count;
            _points.Add(p);
            _map[key] = idx;
            return idx;
        }

        public Point2D Point(int index) => _points[index];
    }
}
