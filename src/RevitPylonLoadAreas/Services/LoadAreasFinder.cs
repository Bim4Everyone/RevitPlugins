using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonLoadAreas.Models;
using RevitPylonLoadAreas.Models.Geometry;
using RevitPylonLoadAreas.Models.Geometry.Voronoi;

namespace RevitPylonLoadAreas.Services;

internal sealed class LoadAreasFinder {
    private readonly SystemConfig _config;
    private readonly RevitRepository _repo;
    private readonly VoronoiBuilder _voronoiBuilder;

    public LoadAreasFinder(
        SystemConfig config,
        RevitRepository repo,
        VoronoiBuilder voronoiBuilder) {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _voronoiBuilder = voronoiBuilder ?? throw new ArgumentNullException(nameof(voronoiBuilder));
    }

    public ICollection<LoadArea> Process(Floor floor, ICollection<FamilyInstance> pylons, ICollection<Wall> walls) {
        var floorData = new FloorVoronoiData(floor, _repo, _config.GetOpeningMinArea());
        var sites = GetSites(floorData, pylons, walls);
        var elementsCells = _voronoiBuilder.Build(sites, new BoundingBoxXY(floorData.Floor.GetBoundingBox()))
            .GroupBy(c => c.Site.Element.Id)
            .ToArray();

        List<LoadArea> loadAreas = [];
        foreach(var elementCells in elementsCells) {
            var cells = elementCells.ToArray();
            IList<CurveLoop> loops;
            var element = cells[0].Site.Element;
            if(cells.Length == 1) {
                loops = floorData.Clip(cells[0]);
            } else {
                loops = floorData.Clip(cells);
            }

            loadAreas.Add(new LoadArea(element, _repo, loops));
        }

        return loadAreas;
    }

    private IList<VoronoiSite> GetSites(
        FloorVoronoiData floorData,
        ICollection<FamilyInstance> pylons,
        ICollection<Wall> walls) {
        List<VoronoiSite> sites = [];

        foreach(var pylon in pylons) {
            var pylonPoint = new XY(((LocationPoint) pylon.Location).Point);
            if(floorData.IsInside(pylonPoint)) {
                sites.Add(new VoronoiSite(pylonPoint, pylon));
            }
        }

        foreach(var wall in walls) {
            var wallPoints = GetWallPoints(wall);
            sites.AddRange(wallPoints.Where(floorData.IsInside).Select(p => new VoronoiSite(p, wall)));
        }

        return sites;
    }

    private ICollection<XY> GetWallPoints(Wall wall) {
        var curve = ((LocationCurve) wall.Location).Curve;
        double length = curve.Length;
        if(length < _config.GetWallsTessellationStep()) {
            return [new XY(curve.GetEndPoint(0))];
        }

        int segments = (int) Math.Ceiling(length / _config.GetWallsTessellationStep());
        List<XY> points = [];
        for(int i = 0; i <= segments; i++) {
            double t = i / (double) segments;
            var p = curve.Evaluate(t, true);
            points.Add(new XY(p));
        }

        return points;
    }
}
