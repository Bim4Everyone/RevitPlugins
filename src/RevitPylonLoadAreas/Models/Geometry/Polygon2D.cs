using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitPylonLoadAreas.Models.Geometry;

/// <summary>
/// Простой полигон с (возможно) внутренними дырами.
/// Внешний контур задается против часовой стрелки (положительная знаковая площадь),
/// контуры дыр — по часовой стрелке.
/// Реализация толерантна к нарушению этого правила: при необходимости ориентация исправляется методами <see cref="EnsureCcw"/>/<see cref="EnsureCw"/>.
/// </summary>
internal sealed class Polygon2D {
    public Polygon2D(IReadOnlyList<Point2D> outerRing, IReadOnlyList<IReadOnlyList<Point2D>> holes = null) {
        if(outerRing == null) {
            throw new ArgumentNullException(nameof(outerRing));
        }
        OuterRing = outerRing;
        Holes = holes ?? Array.Empty<IReadOnlyList<Point2D>>();
    }

    /// <summary>
    /// Внешний контур. Контур замкнут неявно: последняя точка к первой.
    /// </summary>
    public IReadOnlyList<Point2D> OuterRing { get; }

    /// <summary>
    /// Внутренние контуры (дыры). Каждый контур замкнут неявно.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<Point2D>> Holes { get; }

    /// <summary>
    /// Полная площадь полигона = |площадь внешнего| - сумма |площадей дыр|.
    /// </summary>
    public double Area {
        get {
            double a = Math.Abs(SignedRingArea(OuterRing));
            foreach(var hole in Holes) {
                a -= Math.Abs(SignedRingArea(hole));
            }
            return Math.Max(0, a);
        }
    }

    public BoundingBox2D Bounds => BoundingBox2D.FromPoints(OuterRing);

    /// <summary>
    /// Знаковая площадь замкнутого контура по формуле трапеций (shoelace).
    /// Положительная — против часовой стрелки, отрицательная — по часовой.
    /// </summary>
    public static double SignedRingArea(IReadOnlyList<Point2D> ring) {
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

    /// <summary>
    /// Возвращает копию контура, развернутую против часовой стрелки.
    /// </summary>
    public static IReadOnlyList<Point2D> EnsureCcw(IReadOnlyList<Point2D> ring) {
        if(SignedRingArea(ring) < 0) {
            var reversed = new List<Point2D>(ring);
            reversed.Reverse();
            return reversed;
        }
        return ring;
    }

    /// <summary>
    /// Возвращает копию контура, развернутую по часовой стрелке.
    /// </summary>
    public static IReadOnlyList<Point2D> EnsureCw(IReadOnlyList<Point2D> ring) {
        if(SignedRingArea(ring) > 0) {
            var reversed = new List<Point2D>(ring);
            reversed.Reverse();
            return reversed;
        }
        return ring;
    }

    /// <summary>
    /// Тест "точка внутри полигона" с учетом дыр. Точки строго внутри возвращают true,
    /// точки на границе — поведение не определено (используются для информации, точные кейсы решает вызывающий код).
    /// </summary>
    public bool Contains(Point2D point) {
        if(!RingContains(OuterRing, point)) {
            return false;
        }
        foreach(var hole in Holes) {
            if(RingContains(hole, point)) {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Классический ray-casting тест "точка в простом многоугольнике".
    /// </summary>
    public static bool RingContains(IReadOnlyList<Point2D> ring, Point2D point) {
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

    /// <summary>
    /// Итератор по ребрам контура (a -> b).
    /// </summary>
    public static IEnumerable<(Point2D A, Point2D B)> RingEdges(IReadOnlyList<Point2D> ring) {
        int n = ring.Count;
        for(int i = 0; i < n; i++) {
            yield return (ring[i], ring[(i + 1) % n]);
        }
    }

    /// <summary>
    /// Все ребра полигона (внешний контур + дыры).
    /// </summary>
    public IEnumerable<(Point2D A, Point2D B)> AllEdges() {
        foreach(var e in RingEdges(OuterRing)) {
            yield return e;
        }
        foreach(var hole in Holes) {
            foreach(var e in RingEdges(hole)) {
                yield return e;
            }
        }
    }

    public Polygon2D WithoutSmallHoles(double minHoleArea) {
        if(Holes.Count == 0) {
            return this;
        }
        var kept = Holes
            .Where(h => Math.Abs(SignedRingArea(h)) >= minHoleArea)
            .ToList();
        return new Polygon2D(OuterRing, kept);
    }
}
