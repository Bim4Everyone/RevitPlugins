using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonLoadAreas.Models.Algorithms;
using RevitPylonLoadAreas.Models.Geometry;

namespace RevitPylonLoadAreas.Models;

/// <summary>
/// Оркестратор: собирает контур плиты, вытягивает точки стен, строит Вороного и
/// возвращает грузовые площади для каждого пилона.
/// </summary>
internal sealed class PylonLoadAreasProcessor {
    private readonly PluginConfig _config;
    private readonly List<string> _warnings = new();

    public PylonLoadAreasProcessor(PluginConfig config) {
        _config = config;
    }

    public IReadOnlyList<string> Warnings => _warnings;

    public PylonLoadAreasResult Run(
        IReadOnlyList<Floor> floors,
        IReadOnlyList<FamilyInstance> pylons,
        IReadOnlyList<Wall> walls) {

        var floorExtractor = new FloorOutlineExtractor();
        var floorPieces = floorExtractor.Extract(floors);
        _warnings.AddRange(floorExtractor.Warnings);

        var unioned = PolygonUnion.Union(floorPieces);
        var floorOutline = unioned
            .Select(p => p.WithoutSmallHoles(_config.HoleAreaThresholdSqFeet))
            .ToList();

        double slabElevation = floorExtractor.SlabElevation ?? 0.0;

        if(floorOutline.Count == 0) {
            return new PylonLoadAreasResult(
                new Dictionary<ElementId, LoadArea>(),
                floorOutline,
                slabElevation);
        }

        var floorBounds = floorOutline[0].Bounds;
        for(int i = 1; i < floorOutline.Count; i++) {
            floorBounds = floorBounds.Union(floorOutline[i].Bounds);
        }
        double margin = floorBounds.Diagonal * _config.WorldRectMarginRatio;
        var worldBounds = floorBounds.Expanded(margin);

        var sites = new List<Point2D>();
        var siteIsPylon = new List<bool>();
        var sitePylonIds = new List<ElementId>();

        foreach(var pylon in pylons) {
            var center = GetPylonCenter(pylon);
            if(center == null) {
                _warnings.Add($"У пилона [{pylon.Id}] не получилось вычислить центр — элемент пропущен.");
                continue;
            }
            if(!IsInsideAny(center.Value, floorOutline)) {
                _warnings.Add($"Пилон [{pylon.Id}] находится вне контура плиты — элемент пропущен.");
                continue;
            }
            sites.Add(center.Value);
            siteIsPylon.Add(true);
            sitePylonIds.Add(pylon.Id);
        }

        var wallSampler = new WallSampler(_config.WallSampleStepFeet);
        var wallPoints = wallSampler.Sample(walls);
        _warnings.AddRange(wallSampler.Warnings);
        foreach(var wp in wallPoints) {
            sites.Add(wp);
            siteIsPylon.Add(false);
            sitePylonIds.Add(null);
        }

        var dedupSites = new List<Point2D>(sites.Count);
        var dedupIsPylon = new List<bool>(sites.Count);
        var dedupPylonIds = new List<ElementId>(sites.Count);
        var seen = new HashSet<(long, long)>();
        for(int i = 0; i < sites.Count; i++) {
            var key = ((long) System.Math.Round(sites[i].X / GeometryTolerance.Model),
                       (long) System.Math.Round(sites[i].Y / GeometryTolerance.Model));
            if(seen.Add(key)) {
                dedupSites.Add(sites[i]);
                dedupIsPylon.Add(siteIsPylon[i]);
                dedupPylonIds.Add(sitePylonIds[i]);
            } else if(siteIsPylon[i]) {
                _warnings.Add($"Пилон [{sitePylonIds[i]}] схлопнулся с другим сайтом (расстояние меньше допуска).");
            }
        }

        var voronoi = new VoronoiBuilder();
        var cells = voronoi.Build(dedupSites, worldBounds);

        var byPylon = new Dictionary<ElementId, LoadArea>();
        for(int i = 0; i < dedupSites.Count; i++) {
            if(!dedupIsPylon[i]) {
                continue;
            }
            var cell = cells[i];
            if(cell == null || cell.Count < 3) {
                byPylon[dedupPylonIds[i]] = new LoadArea(dedupPylonIds[i], new List<Polygon2D>());
                continue;
            }
            var clipped = ConvexClipper.Intersect(cell, floorOutline);
            byPylon[dedupPylonIds[i]] = new LoadArea(dedupPylonIds[i], clipped);
        }

        return new PylonLoadAreasResult(byPylon, floorOutline, slabElevation);
    }

    private static Point2D? GetPylonCenter(FamilyInstance pylon) {
        if(pylon.Location is LocationPoint lp) {
            return new Point2D(lp.Point.X, lp.Point.Y);
        }
        var bbox = pylon.get_BoundingBox(null);
        if(bbox == null) {
            return null;
        }
        return new Point2D((bbox.Min.X + bbox.Max.X) * 0.5, (bbox.Min.Y + bbox.Max.Y) * 0.5);
    }

    private static bool IsInsideAny(Point2D point, IReadOnlyList<Polygon2D> outline) {
        foreach(var p in outline) {
            if(p.Contains(point)) {
                return true;
            }
        }
        return false;
    }
}
