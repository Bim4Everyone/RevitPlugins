using System;
using System.Collections.Generic;
using System.Linq;

using RevitPylonLoadAreas.Models.Geometry;
using RevitPylonLoadAreas.Models.Geometry.Voronoi;

namespace RevitPylonLoadAreas.Services;

internal sealed class VoronoiBuilder {
    public ICollection<VoronoiCell> Build(IList<VoronoiSite> sites) {
        if(sites == null) {
            throw new ArgumentNullException(nameof(sites));
        }

        if(sites.Count <= 1) {
            throw new ArgumentOutOfRangeException(nameof(sites));
        }

        var cells = new List<VoronoiCell>(sites.Count);
        var sitePoints = sites.Select(s => s.Point).ToArray();
        var delaunay = new BowyerWatsonDelaunay();
        int[] siteIndices = delaunay.Triangulate(sitePoints);

        var trianglesBySite = new Dictionary<int, List<int>>();
        for(int t = 0; t < delaunay.Triangles.Count; t++) {
            var tri = delaunay.Triangles[t];
            AddTriangleToSite(trianglesBySite, tri.V0, t);
            AddTriangleToSite(trianglesBySite, tri.V1, t);
            AddTriangleToSite(trianglesBySite, tri.V2, t);
        }

        for(int s = 0; s < sites.Count; s++) {
            int siteIndex = siteIndices[s];
            IList<XY> cellRing;
            if(trianglesBySite.TryGetValue(siteIndex, out var tris)) {
                var ordered = OrderCircumcentersAroundSite(delaunay, tris, sitePoints[s]);
                if(ordered.Count >= 3) {
                    cellRing = ordered;
                } else {
                    continue;
                }
            } else {
                continue;
            }

            cells.Add(new VoronoiCell(new Polygon2D(cellRing), sites[s]));
        }

        return cells;
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
}
