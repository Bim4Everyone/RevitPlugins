using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitPylonLoadAreas.Models;
using RevitPylonLoadAreas.Models.Geometry;
using RevitPylonLoadAreas.Models.Geometry.Voronoi;

namespace RevitPylonLoadAreas.Services;

internal sealed class LoadAreasFinder {
    private readonly SystemConfig _config;
    private readonly RevitRepository _repo;
    private readonly VoronoiBuilder _voronoiBuilder;
    private readonly CurveLoopsSimplifier _simplifier;

    public LoadAreasFinder(
        SystemConfig config,
        RevitRepository repo,
        VoronoiBuilder voronoiBuilder) {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _voronoiBuilder = voronoiBuilder ?? throw new ArgumentNullException(nameof(voronoiBuilder));
        _simplifier = new CurveLoopsSimplifier();
    }

    public ICollection<LoadArea> Process(
        ICollection<Floor> floors,
        ICollection<FamilyInstance> pylons,
        ICollection<Wall> walls) {
        if(floors is null
           || floors.Count == 0) {
            throw new ArgumentOutOfRangeException(nameof(floors));
        }

        var floorsData = floors
            .Select(f => new FloorVoronoiData(f, _repo, _config.GetOpeningMinArea()))
            .ToArray();
        var sites = GetSites(floorsData, pylons, walls);
        var elementsCells = _voronoiBuilder.Build(sites, GetBoundingBox(floors))
            .GroupBy(c => c.Site.Element.Id)
            .ToArray();

        List<LoadArea> loadAreas = [];
        foreach(var elementCells in elementsCells) {
            var cells = elementCells.ToArray();
            IList<CurveLoop> loops;
            var element = cells[0].Site.Element;
            if(cells.Length == 1) {
                loops = Clip(cells[0], floorsData);
            } else {
                loops = Clip(cells, floorsData);
            }

            loadAreas.Add(new LoadArea(element, _repo, loops));
        }

        return loadAreas;
    }

    private BoundingBoxXY GetBoundingBox(ICollection<Floor> floors) {
        var bboxXyz = floors
            .Select(f => f.GetBoundingBox())
            .ToArray()
            .CreateUnitedBoundingBox();
        return new BoundingBoxXY(bboxXyz);
    }

    private IList<CurveLoop> Clip(VoronoiCell cell, FloorVoronoiData[] floorsData) {
        var cellSolid = _repo.CreateSolid(1, cell.Polygon.AsCurveLoop());
        var floorsUnitedSolid = _repo.CreateUnitedSolid(floorsData.Select(f => f.GetVoronoiSolid()).ToArray());
        return GetIntersectionLoops(cellSolid, floorsUnitedSolid);
    }

    private IList<CurveLoop> Clip(VoronoiCell[] wallCells, FloorVoronoiData[] floorsData) {
        var cellUnitedSolid = _repo.CreateUnitedSolid(
            wallCells.Select(c => _repo.CreateSolid(1, c.Polygon.AsCurveLoop()))
                .ToArray());
        var floorsUnitedSolid = _repo.CreateUnitedSolid(floorsData.Select(f => f.GetVoronoiSolid()).ToArray());
        return GetIntersectionLoops(cellUnitedSolid, floorsUnitedSolid);
    }

    private IList<CurveLoop> GetIntersectionLoops(Solid left, Solid right) {
        var intersection = _repo.Intersect(left, right);
        var bottomFaces = _repo.GetBottomFaces(intersection);
        return bottomFaces.SelectMany(f => _simplifier.GetEdgesAsSimplifiedCurveLoops(f)).ToArray();
    }

    private IList<VoronoiSite> GetSites(
        ICollection<FloorVoronoiData> floorsData,
        ICollection<FamilyInstance> pylons,
        ICollection<Wall> walls) {
        List<VoronoiSite> sites = [];

        foreach(var pylon in pylons) {
            var pylonPoint = new XY(((LocationPoint) pylon.Location).Point);
            if(floorsData.Any(fd => fd.IsInside(pylonPoint))) {
                sites.Add(new VoronoiSite(pylonPoint, pylon));
            }
        }

        foreach(var wall in walls) {
            var wallPoints = GetWallPoints(wall);
            sites.AddRange(
                wallPoints.Where(p => floorsData.Any(fd => fd.IsInside(p))).Select(p => new VoronoiSite(p, wall)));
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
