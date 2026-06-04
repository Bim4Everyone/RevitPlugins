using System;
using System.Collections.Generic;
using System.Linq;

using RevitPylonLoadAreas.Models.Geometry;

namespace RevitPylonLoadAreas.Models.Algorithms;

/// <summary>
/// Пересечение выпуклого полигона (клиппер) с произвольным полигоном
/// (subject) с возможными дырами.
///
/// Реализация: последовательное клиппирование подходящего ring-полигона
/// каждой полуплоскостью, заданной направленным ребром выпуклого клиппера
/// (классический Сазерленд-Ходжман для одного ring-полигона), с последующей
/// расщепкой "вырожденных" самокасаний результата на независимые контуры.
///
/// Алгоритм корректно работает потому, что клиппер выпуклый: каждая полуплоскость
/// не "разрывает" уже отклиппированный полигон на разрозненные части произвольным
/// способом — образующиеся самокасания соединены прямой линией клиппирующего ребра
/// и легко разрезаются в постобработке.
/// </summary>
internal static class ConvexClipper {
    /// <summary>
    /// Пересекает выпуклый клиппер с одним subject-полигоном.
    /// </summary>
    public static List<Polygon2D> Intersect(IReadOnlyList<Point2D> convexClip, Polygon2D subject) {
        var clip = Polygon2D.EnsureCcw(convexClip);
        if(clip.Count < 3) {
            return new List<Polygon2D>();
        }

        var outerRings = SplitRingToSimpleRings(
            ClipRingAgainstConvex(subject.OuterRing, clip));
        if(outerRings.Count == 0) {
            return new List<Polygon2D>();
        }

        var holeRings = new List<List<Point2D>>();
        foreach(var hole in subject.Holes) {
            holeRings.AddRange(SplitRingToSimpleRings(ClipRingAgainstConvex(hole, clip)));
        }

        var result = new List<Polygon2D>();
        foreach(var outerRaw in outerRings) {
            var outer = Polygon2D.EnsureCcw(outerRaw);
            var assignedHoles = new List<IReadOnlyList<Point2D>>();
            for(int i = holeRings.Count - 1; i >= 0; i--) {
                var holeProbe = holeRings[i][0];
                if(Polygon2D.RingContains(outer, holeProbe)) {
                    assignedHoles.Add(Polygon2D.EnsureCw(holeRings[i]));
                    holeRings.RemoveAt(i);
                }
            }
            if(Math.Abs(Polygon2D.SignedRingArea(outer)) < GeometryTolerance.Area) {
                continue;
            }
            result.Add(new Polygon2D(outer, assignedHoles));
        }
        return result;
    }

    /// <summary>
    /// Пересекает выпуклый клиппер с набором subject-полигонов.
    /// </summary>
    public static List<Polygon2D> Intersect(IReadOnlyList<Point2D> convexClip, IReadOnlyList<Polygon2D> subjects) {
        var result = new List<Polygon2D>();
        foreach(var subj in subjects) {
            result.AddRange(Intersect(convexClip, subj));
        }
        return result;
    }

    /// <summary>
    /// Применяет к одному ring-полигону последовательное клиппирование всеми ребрами
    /// выпуклого клиппера. Результат — список вершин одного "потенциально вырожденного" ring-полигона.
    /// </summary>
    private static List<Point2D> ClipRingAgainstConvex(IReadOnlyList<Point2D> ring, IReadOnlyList<Point2D> convexCcw) {
        if(ring == null || ring.Count < 3) {
            return new List<Point2D>();
        }
        var input = new List<Point2D>(ring);
        int m = convexCcw.Count;
        for(int i = 0; i < m && input.Count > 0; i++) {
            var a = convexCcw[i];
            var b = convexCcw[(i + 1) % m];
            input = ClipRingByHalfPlane(input, a, b);
        }
        return input;
    }

    /// <summary>
    /// Сазерленд-Ходжман: оставляет точки слева от направленного ребра a→b.
    /// </summary>
    private static List<Point2D> ClipRingByHalfPlane(List<Point2D> ring, Point2D a, Point2D b) {
        var output = new List<Point2D>();
        if(ring.Count == 0) {
            return output;
        }
        for(int i = 0; i < ring.Count; i++) {
            var current = ring[i];
            var previous = ring[(i - 1 + ring.Count) % ring.Count];
            double sideCurrent = SideOf(a, b, current);
            double sidePrevious = SideOf(a, b, previous);
            bool curIn = sideCurrent >= -GeometryTolerance.Model;
            bool prevIn = sidePrevious >= -GeometryTolerance.Model;

            if(curIn) {
                if(!prevIn) {
                    if(TryIntersect(previous, current, a, b, out var crossing)) {
                        output.Add(crossing);
                    }
                }
                output.Add(current);
            } else if(prevIn) {
                if(TryIntersect(previous, current, a, b, out var crossing)) {
                    output.Add(crossing);
                }
            }
        }
        return output;
    }

    /// <summary>
    /// Положительное значение — точка слева от направленного ребра, отрицательное — справа.
    /// </summary>
    private static double SideOf(Point2D a, Point2D b, Point2D p) {
        return (b.X - a.X) * (p.Y - a.Y) - (b.Y - a.Y) * (p.X - a.X);
    }

    private static bool TryIntersect(Point2D p1, Point2D p2, Point2D a, Point2D b, out Point2D result) {
        double rx = p2.X - p1.X;
        double ry = p2.Y - p1.Y;
        double sx = b.X - a.X;
        double sy = b.Y - a.Y;
        double denom = rx * sy - ry * sx;
        if(Math.Abs(denom) < GeometryTolerance.Model) {
            result = default;
            return false;
        }
        double t = ((a.X - p1.X) * sy - (a.Y - p1.Y) * sx) / denom;
        result = new Point2D(p1.X + t * rx, p1.Y + t * ry);
        return true;
    }

    /// <summary>
    /// Разрезает потенциально вырожденный ring-полигон с самокасаниями
    /// (повторяющиеся вершины) на набор простых ring-полигонов.
    /// </summary>
    private static List<List<Point2D>> SplitRingToSimpleRings(List<Point2D> ring) {
        var result = new List<List<Point2D>>();
        if(ring == null || ring.Count < 3) {
            return result;
        }

        var cleaned = RemoveConsecutiveDuplicates(ring);
        if(cleaned.Count < 3) {
            return result;
        }

        SplitRecursive(cleaned, result);
        return result;
    }

    private static void SplitRecursive(List<Point2D> ring, List<List<Point2D>> sink) {
        var firstSeen = new Dictionary<(long, long), int>();
        for(int i = 0; i < ring.Count; i++) {
            var key = Quantize(ring[i]);
            if(firstSeen.TryGetValue(key, out int firstIndex)) {
                var inner = ring.GetRange(firstIndex, i - firstIndex);
                var outer = new List<Point2D>(ring.GetRange(0, firstIndex));
                outer.AddRange(ring.GetRange(i, ring.Count - i));

                if(inner.Count >= 3 && Math.Abs(Polygon2D.SignedRingArea(inner)) > GeometryTolerance.Area) {
                    SplitRecursive(inner, sink);
                }
                if(outer.Count >= 3 && Math.Abs(Polygon2D.SignedRingArea(outer)) > GeometryTolerance.Area) {
                    SplitRecursive(outer, sink);
                }
                return;
            }
            firstSeen[key] = i;
        }
        if(ring.Count >= 3 && Math.Abs(Polygon2D.SignedRingArea(ring)) > GeometryTolerance.Area) {
            sink.Add(ring);
        }
    }

    private static List<Point2D> RemoveConsecutiveDuplicates(List<Point2D> ring) {
        var result = new List<Point2D>(ring.Count);
        for(int i = 0; i < ring.Count; i++) {
            var prev = result.Count > 0 ? result[result.Count - 1] : ring[ring.Count - 1];
            if(!ring[i].IsAlmostEqual(prev)) {
                result.Add(ring[i]);
            }
        }
        while(result.Count > 1 && result[0].IsAlmostEqual(result[result.Count - 1])) {
            result.RemoveAt(result.Count - 1);
        }
        return result;
    }

    private static (long, long) Quantize(Point2D p) {
        double tol = GeometryTolerance.Model;
        return ((long) Math.Round(p.X / tol), (long) Math.Round(p.Y / tol));
    }
}
