using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using dosymep.SimpleServices;

using RevitPylonLoadAreas.Models;
using RevitPylonLoadAreas.Models.Geometry;
using RevitPylonLoadAreas.Models.Geometry.Voronoi;

namespace RevitPylonLoadAreas.Services;

internal sealed class LoadAreasFinder {
    private readonly SystemConfig _config;
    private readonly VoronoiBuilder _voronoiBuilder;

    public LoadAreasFinder(
        SystemConfig config,
        VoronoiBuilder voronoiBuilder) {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _voronoiBuilder = voronoiBuilder ?? throw new ArgumentNullException(nameof(voronoiBuilder));
    }

    public ICollection<LoadArea> Process(Floor floor, ICollection<FamilyInstance> pylons, ICollection<Wall> walls) {
        var floorData = new FloorVoronoiData(floor, _config.OpeningMinArea);
        var sites = GetSites(floorData, pylons, walls);
        var voronoiCells = _voronoiBuilder.Build(sites).ToArray();

        var pylonIds = pylons.Select(pylon => pylon.Id).ToHashSet();
        List<LoadArea> loadAreas = [];
        foreach(var cell in voronoiCells) {
            // работаем только с грузовыми площадями пилонов
            if(!pylonIds.Contains(cell.Site.Element.Id)) {
                continue;
            }

            var areaLoops = floorData.Clip(cell);
            loadAreas.Add(new LoadArea(cell.Site.Element, areaLoops));
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
        if(length < _config.WallsTessellationStep) {
            return [new XY(curve.GetEndPoint(0))];
        }

        int segments = (int) Math.Ceiling(length / _config.WallsTessellationStep);
        List<XY> points = [];
        for(int i = 0; i <= segments; i++) {
            double t = i / (double) segments;
            var p = curve.Evaluate(t, true);
            points.Add(new XY(p));
        }

        return points;
    }
}
