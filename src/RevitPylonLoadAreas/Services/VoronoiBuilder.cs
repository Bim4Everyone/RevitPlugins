using System;
using System.Collections.Generic;
using System.Linq;

using RevitPylonLoadAreas.Models.Geometry;
using RevitPylonLoadAreas.Models.Geometry.Voronoi;

namespace RevitPylonLoadAreas.Services;

internal sealed class VoronoiBuilder {
    public VoronoiResult Build(ICollection<VoronoiSite> sites, BoundingBoxXY boundaries) {
        if(sites == null) {
            throw new ArgumentNullException(nameof(sites));
        }

        var siteList = sites.ToList();
        var cells = new List<VoronoiCell>(siteList.Count);

        if(siteList.Count == 0) {
            return new VoronoiResult(cells);
        }

        var worldRect = WorldRect(boundaries);

        if(siteList.Count == 1) {
            cells.Add(new VoronoiCell(new Polygon2D(worldRect), siteList[0]));
            return new VoronoiResult(cells);
        }

        var sitePoints = siteList.Select(s => s.Point).ToList();
        var delaunay = new BowyerWatsonDelaunay();
        double margin = Math.Max(boundaries.Diagonal * 10.0, 1.0);
        var siteIndices = delaunay.Triangulate(sitePoints, boundaries, margin);

        var trianglesBySite = new Dictionary<int, List<int>>();
        for(int t = 0; t < delaunay.Triangles.Count; t++) {
            var tri = delaunay.Triangles[t];
            AddTriangleToSite(trianglesBySite, tri.V0, t);
            AddTriangleToSite(trianglesBySite, tri.V1, t);
            AddTriangleToSite(trianglesBySite, tri.V2, t);
        }

        for(int s = 0; s < siteList.Count; s++) {
            int siteIndex = siteIndices[s];
            IList<XY> cellRing;
            if(trianglesBySite.TryGetValue(siteIndex, out var tris)) {
                var ordered = OrderCircumcentersAroundSite(delaunay, tris, sitePoints[s]);
                if(ordered.Count >= 3) {
                    cellRing = ConvexClipPolygonByRect(ordered, worldRect);
                } else {
                    cellRing = new List<XY>();
                }
            } else {
                cellRing = new List<XY>();
            }

            cells.Add(new VoronoiCell(new Polygon2D(cellRing), siteList[s]));
        }

        return new VoronoiResult(cells);
    }

    private void AddTriangleToSite(Dictionary<int, List<int>> map, int siteIndex, int triangleIndex) {
        if(!map.TryGetValue(siteIndex, out var list)) {
            list = new List<int>();
            map[siteIndex] = list;
        }

        list.Add(triangleIndex);
    }

    private List<XY> OrderCircumcentersAroundSite(
        BowyerWatsonDelaunay delaunay,
        List<int> triangleIndices,
        XY site) {
        var entries = new List<(double Angle, XY Center)>(triangleIndices.Count);
        foreach(int ti in triangleIndices) {
            var tri = delaunay.Triangles[ti];
            var center = tri.Circumcenter;
            double angle = Math.Atan2(center.Y - site.Y, center.X - site.X);
            entries.Add((angle, center));
        }

        entries.Sort((a, b) => a.Angle.CompareTo(b.Angle));
        var ordered = new List<XY>(entries.Count);
        foreach(var e in entries) {
            if(ordered.Count > 0
               && ordered[ordered.Count - 1].IsAlmostEqual(e.Center)) {
                continue;
            }

            ordered.Add(e.Center);
        }

        if(ordered.Count > 1
           && ordered[0].IsAlmostEqual(ordered[ordered.Count - 1])) {
            ordered.RemoveAt(ordered.Count - 1);
        }

        return ordered;
    }

    private List<XY> ConvexClipPolygonByRect(List<XY> polygon, IList<XY> rect) {
        var input = polygon;
        for(int i = 0; i < rect.Count; i++) {
            var a = rect[i];
            var b = rect[(i + 1) % rect.Count];
            input = ClipByHalfPlane(input, a, b);
            if(input.Count == 0) {
                return new List<XY>();
            }
        }

        return input;
    }

    private List<XY> ClipByHalfPlane(List<XY> ring, XY a, XY b) {
        var output = new List<XY>();
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
                    output.Add(IntersectEdges(previous, current, a, b));
                }

                output.Add(current);
            } else if(prevIn) {
                output.Add(IntersectEdges(previous, current, a, b));
            }
        }

        return output;
    }

    private XY IntersectEdges(XY p1, XY p2, XY a, XY b) {
        double rx = p2.X - p1.X;
        double ry = p2.Y - p1.Y;
        double sx = b.X - a.X;
        double sy = b.Y - a.Y;
        double denom = rx * sy - ry * sx;
        if(Math.Abs(denom) < GeometryTolerance.Model) {
            return p2;
        }

        double t = ((a.X - p1.X) * sy - (a.Y - p1.Y) * sx) / denom;
        return new XY(p1.X + t * rx, p1.Y + t * ry);
    }

    private List<XY> WorldRect(BoundingBoxXY b) {
        return new List<XY> {
            new(b.Min.X, b.Min.Y),
            new(b.Max.X, b.Min.Y),
            new(b.Max.X, b.Max.Y),
            new(b.Min.X, b.Max.Y)
        };
    }
}
