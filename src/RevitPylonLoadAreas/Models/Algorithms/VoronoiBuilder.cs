using System;
using System.Collections.Generic;
using System.Linq;

using RevitPylonLoadAreas.Models.Geometry;

namespace RevitPylonLoadAreas.Models.Algorithms;

/// <summary>
/// Строит ячейки диаграммы Вороного как двойственный граф к Делоновской триангуляции.
/// Для каждой исходной точки возвращает многоугольник её ячейки, обрезанный
/// "мировым прямоугольником" (для приграничных, изначально бесконечных ячеек).
/// </summary>
internal sealed class VoronoiBuilder {
    /// <summary>
    /// Строит ячейки Вороного по списку сайтов.
    /// </summary>
    /// <param name="sites">Исходные точки.</param>
    /// <param name="worldBounds">Прямоугольник для отсечения бесконечных ячеек на границе.</param>
    /// <returns>Список ячеек, в том же порядке, что и <paramref name="sites"/>.</returns>
    public List<IReadOnlyList<Point2D>> Build(IReadOnlyList<Point2D> sites, BoundingBox2D worldBounds) {
        var result = new List<IReadOnlyList<Point2D>>(sites.Count);
        if(sites.Count == 0) {
            return result;
        }
        if(sites.Count == 1) {
            result.Add(WorldRect(worldBounds));
            return result;
        }

        var delaunay = new BowyerWatsonDelaunay();
        var triangulationMargin = Math.Max(worldBounds.Diagonal * 10.0, 1.0);
        var siteIndices = delaunay.Triangulate(sites, worldBounds, triangulationMargin);

        var trianglesBySite = new Dictionary<int, List<int>>();
        for(int t = 0; t < delaunay.Triangles.Count; t++) {
            var tri = delaunay.Triangles[t];
            AddTriangleToSite(trianglesBySite, tri.V0, t);
            AddTriangleToSite(trianglesBySite, tri.V1, t);
            AddTriangleToSite(trianglesBySite, tri.V2, t);
        }

        var worldRect = WorldRect(worldBounds);

        for(int s = 0; s < sites.Count; s++) {
            int siteIndex = siteIndices[s];
            if(!trianglesBySite.TryGetValue(siteIndex, out var tris)) {
                result.Add(Array.Empty<Point2D>());
                continue;
            }

            var sitePoint = sites[s];
            var orderedCenters = OrderTrianglesAroundSite(delaunay, tris, sitePoint);
            if(orderedCenters.Count < 3) {
                result.Add(Array.Empty<Point2D>());
                continue;
            }

            var clipped = ConvexClipPolygonByRect(orderedCenters, worldRect);
            result.Add(clipped);
        }
        return result;
    }

    private static void AddTriangleToSite(Dictionary<int, List<int>> map, int siteIndex, int triangleIndex) {
        if(!map.TryGetValue(siteIndex, out var list)) {
            list = new List<int>();
            map[siteIndex] = list;
        }
        list.Add(triangleIndex);
    }

    /// <summary>
    /// Возвращает циркумцентры треугольников, инцидентных сайту, в порядке против часовой стрелки.
    /// </summary>
    private static List<Point2D> OrderTrianglesAroundSite(
        BowyerWatsonDelaunay delaunay, List<int> triangleIndices, Point2D site) {
        var entries = new List<(double Angle, Point2D Center)>(triangleIndices.Count);
        foreach(int ti in triangleIndices) {
            var tri = delaunay.Triangles[ti];
            var center = tri.Circumcenter;
            double angle = Math.Atan2(center.Y - site.Y, center.X - site.X);
            entries.Add((angle, center));
        }
        entries.Sort((a, b) => a.Angle.CompareTo(b.Angle));
        var ordered = new List<Point2D>(entries.Count);
        foreach(var e in entries) {
            if(ordered.Count > 0 && ordered[ordered.Count - 1].IsAlmostEqual(e.Center)) {
                continue;
            }
            ordered.Add(e.Center);
        }
        if(ordered.Count > 1 && ordered[0].IsAlmostEqual(ordered[ordered.Count - 1])) {
            ordered.RemoveAt(ordered.Count - 1);
        }
        return ordered;
    }

    /// <summary>
    /// Отсекает многоугольник прямоугольником через четыре полуплоскости (Сазерленд-Ходжман).
    /// Многоугольник на входе предполагается выпуклым — это всегда так для ячеек Вороного.
    /// </summary>
    private static IReadOnlyList<Point2D> ConvexClipPolygonByRect(List<Point2D> polygon, IReadOnlyList<Point2D> rect) {
        var input = polygon;
        for(int i = 0; i < rect.Count; i++) {
            var a = rect[i];
            var b = rect[(i + 1) % rect.Count];
            input = ClipByHalfPlane(input, a, b);
            if(input.Count == 0) {
                return Array.Empty<Point2D>();
            }
        }
        return input;
    }

    private static List<Point2D> ClipByHalfPlane(List<Point2D> ring, Point2D a, Point2D b) {
        var output = new List<Point2D>();
        if(ring.Count == 0) {
            return output;
        }
        for(int i = 0; i < ring.Count; i++) {
            var current = ring[i];
            var previous = ring[(i - 1 + ring.Count) % ring.Count];
            double sideCurrent = (b.X - a.X) * (current.Y - a.Y) - (b.Y - a.Y) * (current.X - a.X);
            double sidePrevious = (b.X - a.X) * (previous.Y - a.Y) - (b.Y - a.Y) * (previous.X - a.X);
            bool curIn = sideCurrent >= -GeometryTolerance.Model;
            bool prevIn = sidePrevious >= -GeometryTolerance.Model;
            if(curIn) {
                if(!prevIn) {
                    output.Add(Intersect(previous, current, a, b));
                }
                output.Add(current);
            } else if(prevIn) {
                output.Add(Intersect(previous, current, a, b));
            }
        }
        return output;
    }

    private static Point2D Intersect(Point2D p1, Point2D p2, Point2D a, Point2D b) {
        double rx = p2.X - p1.X;
        double ry = p2.Y - p1.Y;
        double sx = b.X - a.X;
        double sy = b.Y - a.Y;
        double denom = rx * sy - ry * sx;
        if(Math.Abs(denom) < GeometryTolerance.Model) {
            return p2;
        }
        double t = ((a.X - p1.X) * sy - (a.Y - p1.Y) * sx) / denom;
        return new Point2D(p1.X + t * rx, p1.Y + t * ry);
    }

    private static List<Point2D> WorldRect(BoundingBox2D b) {
        return new List<Point2D> {
            new(b.MinX, b.MinY),
            new(b.MaxX, b.MinY),
            new(b.MaxX, b.MaxY),
            new(b.MinX, b.MaxY)
        };
    }
}
