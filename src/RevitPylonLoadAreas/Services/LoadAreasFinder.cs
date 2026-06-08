using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonLoadAreas.Exceptions;
using RevitPylonLoadAreas.Models;
using RevitPylonLoadAreas.Models.Geometry;
using RevitPylonLoadAreas.Models.Geometry.Voronoi;

namespace RevitPylonLoadAreas.Services;

internal sealed class LoadAreasFinder {
    private const double ExtrusionHeight = 1.0;

    private readonly SystemConfig _config;
    private readonly VoronoiBuilder _voronoiBuilder;

    public LoadAreasFinder(SystemConfig config, VoronoiBuilder voronoiBuilder) {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _voronoiBuilder = voronoiBuilder ?? throw new ArgumentNullException(nameof(voronoiBuilder));
    }

    public ICollection<LoadArea> Process(
        Floor floor,
        IReadOnlyList<FamilyInstance> pylons,
        IReadOnlyList<Wall> walls) {

        var topFace = GetTopFace(floor);
        double slabZ = topFace.Origin.Z;
        var loops = topFace.GetEdgesAsCurveLoops();

        var outerLoop = SelectLongestLoop(loops, out var holeLoops);
        var keptHoleLoops = FilterHolesByArea(holeLoops);

        var floorSolid = BuildFloorSolid(outerLoop, keptHoleLoops);

        var sites = CollectSites(pylons, walls);
        ValidateSiteDistances(sites);

        var bbox = BoundingBoxXY.FromBoundingBoxXYZ(floor.get_BoundingBox(null));
        var worldBounds = bbox.Expanded(bbox.Diagonal * 0.5);
        var voronoi = _voronoiBuilder.Build(sites, worldBounds);

        var loadAreas = new List<LoadArea>(pylons.Count);
        foreach(var cell in voronoi.Cells) {
            if(cell.Site.ElementId == null) {
                continue;
            }
            var pylon = (FamilyInstance) floor.Document.GetElement(cell.Site.ElementId);
            var circuits = ClipCellByFloor(cell.Polygon, floorSolid, slabZ);
            loadAreas.Add(new LoadArea(pylon, circuits));
        }
        return loadAreas;
    }

    private static PlanarFace GetTopFace(Floor floor) {
        var options = new Options {
            ComputeReferences = false,
            IncludeNonVisibleObjects = false,
            DetailLevel = ViewDetailLevel.Fine
        };
        PlanarFace best = null;
        foreach(var go in floor.get_Geometry(options)) {
            best = FindTopFace(go, best);
        }
        if(best == null) {
            throw new FloorTopFaceNotFoundException(floor.Id);
        }
        return best;
    }

    private static PlanarFace FindTopFace(GeometryObject go, PlanarFace best) {
        switch(go) {
            case Solid solid when solid.Volume > 0:
                foreach(Face f in solid.Faces) {
                    if(f is PlanarFace pf && pf.FaceNormal.Z > 0.95) {
                        if(best == null || pf.Origin.Z > best.Origin.Z) {
                            best = pf;
                        }
                    }
                }
                break;
            case GeometryInstance gi:
                foreach(var inner in gi.GetInstanceGeometry()) {
                    best = FindTopFace(inner, best);
                }
                break;
        }
        return best;
    }

    private static CurveLoop SelectLongestLoop(IList<CurveLoop> loops, out List<CurveLoop> holes) {
        if(loops == null || loops.Count == 0) {
            throw new InvalidOperationException("У верхней грани плиты нет ни одного контура.");
        }
        var ordered = loops
            .Select(l => new { Loop = l, Length = l.GetExactLength() })
            .OrderByDescending(x => x.Length)
            .ToList();
        holes = ordered.Skip(1).Select(x => x.Loop).ToList();
        return ordered[0].Loop;
    }

    private List<CurveLoop> FilterHolesByArea(List<CurveLoop> holes) {
        var kept = new List<CurveLoop>();
        foreach(var hole in holes) {
            double area = MeasureLoopArea(hole);
            if(area >= _config.OpeningMinArea) {
                kept.Add(hole);
            }
        }
        return kept;
    }

    private static double MeasureLoopArea(CurveLoop loop) {
        var ccw = EnsureCcw(loop);
        var solid = GeometryCreationUtilities.CreateExtrusionGeometry(
            new List<CurveLoop> { ccw }, XYZ.BasisZ, ExtrusionHeight);
        foreach(Face f in solid.Faces) {
            if(f is PlanarFace pf && pf.FaceNormal.Z > 0.95) {
                return pf.Area;
            }
        }
        return 0;
    }

    private static Solid BuildFloorSolid(CurveLoop outer, List<CurveLoop> holes) {
        var profile = new List<CurveLoop> { EnsureCcw(outer) };
        foreach(var hole in holes) {
            profile.Add(EnsureCw(hole));
        }
        return GeometryCreationUtilities.CreateExtrusionGeometry(profile, XYZ.BasisZ, ExtrusionHeight);
    }

    private List<VoronoiSite> CollectSites(
        IReadOnlyList<FamilyInstance> pylons, IReadOnlyList<Wall> walls) {
        var sites = new List<VoronoiSite>(pylons.Count + walls.Count * 4);
        foreach(var pylon in pylons) {
            sites.Add(new VoronoiSite(GetPylonCenter(pylon), pylon.Id));
        }
        foreach(var wall in walls) {
            foreach(var p in SampleWall(wall)) {
                sites.Add(new VoronoiSite(p, null));
            }
        }
        return sites;
    }

    private static XY GetPylonCenter(FamilyInstance pylon) {
        if(pylon.Location is LocationPoint lp) {
            return new XY(lp.Point);
        }
        var bbox = pylon.get_BoundingBox(null);
        if(bbox != null) {
            return new XY((bbox.Min.X + bbox.Max.X) * 0.5, (bbox.Min.Y + bbox.Max.Y) * 0.5);
        }
        throw new InvalidOperationException($"У пилона [{pylon.Id}] нет позиции (Location/BoundingBox).");
    }

    private IEnumerable<XY> SampleWall(Wall wall) {
        if(wall.Location is not LocationCurve location) {
            yield break;
        }
        var curve = location.Curve;
        double length = curve.Length;
        if(length < GeometryTolerance.Model) {
            yield break;
        }

        int segments = Math.Max(1, (int) Math.Ceiling(length / _config.WallsTessellationStep));
        for(int i = 0; i <= segments; i++) {
            double t = i / (double) segments;
            var p = curve.Evaluate(t, true);
            yield return new XY(p);
        }
    }

    private void ValidateSiteDistances(IList<VoronoiSite> sites) {
        double minSqr = _config.MinSiteDistance * _config.MinSiteDistance;
        for(int i = 0; i < sites.Count; i++) {
            var a = sites[i];
            if(a.ElementId == null) {
                continue;
            }
            for(int j = i + 1; j < sites.Count; j++) {
                var b = sites[j];
                double sqr = a.Point.SqrDistanceTo(b.Point);
                if(sqr >= minSqr) {
                    continue;
                }
                double dist = Math.Sqrt(sqr);
                if(b.ElementId != null) {
                    throw new PylonsTooCloseException(a.ElementId, b.ElementId, dist);
                }
                throw new PylonAndWallTooCloseException(a.ElementId, dist);
            }
        }
    }

    private IList<Polygon2D> ClipCellByFloor(Polygon2D cell, Solid floorSolid, double slabZ) {
        if(cell.Vertices.Count < 3) {
            return new List<Polygon2D>();
        }
        var cellLoop = new Polygon2D(Polygon2D.EnsureCcw(cell.Vertices)).AsCurvLoop(slabZ);
        var cellSolid = GeometryCreationUtilities.CreateExtrusionGeometry(
            new List<CurveLoop> { cellLoop }, XYZ.BasisZ, ExtrusionHeight);

        Solid clipped;
        try {
            clipped = BooleanOperationsUtils.ExecuteBooleanOperation(
                floorSolid, cellSolid, BooleanOperationsType.Intersect);
        } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
            return new List<Polygon2D>();
        }
        if(clipped == null || clipped.Volume <= 0) {
            return new List<Polygon2D>();
        }

        PlanarFace top = null;
        foreach(Face f in clipped.Faces) {
            if(f is PlanarFace pf && pf.FaceNormal.Z > 0.95) {
                if(top == null || pf.Origin.Z > top.Origin.Z) {
                    top = pf;
                }
            }
        }
        if(top == null) {
            return new List<Polygon2D>();
        }

        var resultLoops = top.GetEdgesAsCurveLoops()
            .Select(l => new { Loop = l, Length = l.GetExactLength() })
            .OrderByDescending(x => x.Length)
            .Select(x => x.Loop)
            .ToList();

        var circuits = new List<Polygon2D>(resultLoops.Count);
        for(int i = 0; i < resultLoops.Count; i++) {
            IList<XY> ring = TessellateLoop(resultLoops[i]);
            if(ring.Count < 3) {
                continue;
            }
            ring = i == 0 ? Polygon2D.EnsureCcw(ring) : Polygon2D.EnsureCw(ring);
            circuits.Add(new Polygon2D(ring));
        }
        return circuits;
    }

    private List<XY> TessellateLoop(CurveLoop loop) {
        var result = new List<XY>();
        foreach(var curve in loop) {
            if(curve is Line) {
                result.Add(new XY(curve.GetEndPoint(0)));
                continue;
            }
            int n = Math.Max(1, (int) Math.Ceiling(curve.Length / _config.FloorArcTessellationStep));
            for(int i = 0; i < n; i++) {
                double t = i / (double) n;
                result.Add(new XY(curve.Evaluate(t, true)));
            }
        }
        return result;
    }

    private static CurveLoop EnsureCcw(CurveLoop loop) {
        if(loop.IsCounterclockwise(XYZ.BasisZ)) {
            return loop;
        }
        var copy = CloneLoop(loop);
        copy.Flip();
        return copy;
    }

    private static CurveLoop EnsureCw(CurveLoop loop) {
        if(!loop.IsCounterclockwise(XYZ.BasisZ)) {
            return loop;
        }
        var copy = CloneLoop(loop);
        copy.Flip();
        return copy;
    }

    private static CurveLoop CloneLoop(CurveLoop source) {
        var copy = new CurveLoop();
        foreach(var c in source) {
            copy.Append(c.Clone());
        }
        return copy;
    }
}
