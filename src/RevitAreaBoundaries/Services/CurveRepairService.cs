using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.Revit.DB;
using RevitAreaBoundaries.Models;

namespace RevitAreaBoundaries.Services;

internal class CurveRepairService(SystemPluginConfig systemPluginConfig, RevitRepository revitRepository) {
    private readonly double _tolerance = systemPluginConfig.DefaultTolerance;
    private readonly double _shortCurveTolerance = revitRepository.Application.ShortCurveTolerance;
    private readonly double _cellSizeForIndexMm = systemPluginConfig.DefaultCellsSizeForIndexMm;

    public List<Curve> CleanDuplicateCurves(List<Curve> curves) {
        if (curves == null || curves.Count == 0)
            return [];

        var result = new List<Curve>(curves.Count);
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var curve in curves) {
            string key = BuildCurveKey(curve);
            if (key == null) {
                if (!result.Any(existing => IsEqual(existing, curve))) {
                    result.Add(curve);
                }
                continue;
            }
            if (seen.Add(key)) {
                result.Add(curve);
            }
        }
        return result;
    }

    public List<Curve> RepairContour(List<Curve> curves) {
        if (curves == null || curves.Count == 0)
            return [];

        double minDistance = UnitUtils.ConvertToInternalUnits(systemPluginConfig.DefaultMinDistanceToJoinPointsMm, UnitTypeId.Millimeters);
        double maxDistance = UnitUtils.ConvertToInternalUnits(systemPluginConfig.DefaultMaxDistanceToCreateCurveFineMm, UnitTypeId.Millimeters);

        var result = curves.Select(x => x.Clone()).ToList();
        var additionalCurves = new List<Curve>();

        var index = new CurveSpatialIndex(result, maxDistance);

        for (int curveIndex = 0; curveIndex < result.Count; curveIndex++) {
            if (result[curveIndex] is not Line currentLine) {
                continue;
            }

            bool lineChanged = false;

            for (int endIndex = 0; endIndex < 2; endIndex++) {
                var endpoint = currentLine.GetEndPoint(endIndex);

                if (IsEndpointConnectedIndexed(endpoint, curveIndex, result, index, _tolerance)) {
                    continue;
                }

                XYZ nearestPoint = null;
                double nearestDistance = double.MaxValue;

                var searchBox = BBox.BBoxFromPointXy(endpoint, maxDistance);
                foreach (int candidateIndex in index.Query(searchBox)) {
                    if (candidateIndex == curveIndex) {
                        continue;
                    }

                    var candidate = result[candidateIndex];
                    var projection = candidate.Project(endpoint);
                    if (projection == null) {
                        continue;
                    }
                    if(!(projection.Distance < nearestDistance)) {
                        continue;
                    }
                    nearestDistance = projection.Distance;
                    nearestPoint = projection.XYZPoint;
                }

                if (nearestPoint == null || nearestDistance > maxDistance) {
                    continue;
                }

                if (nearestDistance <= minDistance) {
                    var newLine = ReplaceEndpoint(currentLine, endIndex, nearestPoint);
                    if(newLine == null) {
                        continue;
                    }
                    currentLine = newLine;
                    result[curveIndex] = currentLine;
                    lineChanged = true;
                } else {
                    if (!IsTooShort(endpoint, nearestPoint)) {
                        additionalCurves.Add(Line.CreateBound(endpoint, nearestPoint));
                    }
                }
            }
            if (lineChanged) {
                index = new CurveSpatialIndex(result, maxDistance);
            }
        }
        if (additionalCurves.Count > 0) {
            result.AddRange(additionalCurves);
        }
        return result;
    }

    public List<Curve> GetCurvesConnectedByBothEnds(List<Curve> curves) {
        if (curves == null || curves.Count == 0)
            return [];
        
        double cellSize = UnitUtils.ConvertToInternalUnits(_cellSizeForIndexMm, UnitTypeId.Millimeters);
        
        var index = new CurveSpatialIndex(curves, cellSize);

        var connected = new List<Curve>(curves.Count);
        for (int i = 0; i < curves.Count; i++) {
            var c = curves[i];
            bool c0 = IsEndpointConnectedIndexed(c.GetEndPoint(0), i, curves, index, _tolerance);
            if (!c0) {
                continue;
            }

            bool c1 = IsEndpointConnectedIndexed(c.GetEndPoint(1), i, curves, index, _tolerance);
            if (c1) {
                connected.Add(c);
            }
        }
        return connected;
    }

    private bool IsEndpointConnectedIndexed(XYZ point, int ownerIndex, List<Curve> curves, CurveSpatialIndex index, double tolerance) {
        var probe = BBox.BBoxFromPointXy(point, tolerance);

        foreach (int i in index.Query(probe)) {
            if (i == ownerIndex) {
                continue;
            }

            var curve = curves[i];

            if (PointsEqual(curve.GetEndPoint(0), point)) {
                return true;
            }
            if (PointsEqual(curve.GetEndPoint(1), point)) {
                return true;
            }

            var projection = curve.Project(point);
            if (projection != null && projection.Distance <= tolerance) {
                return true;
            }
        }
        return false;
    }

    private bool PointsEqual(XYZ p1, XYZ p2) {
        return p1.DistanceTo(p2) <= _tolerance;
    }
    
    private bool IsTooShort(XYZ p1, XYZ p2) {
        return p1.DistanceTo(p2) < _shortCurveTolerance;
    }

    private Line ReplaceEndpoint(Line line, int endpointIndex, XYZ newPoint) {
        var p1 = endpointIndex == 0 ? newPoint : line.GetEndPoint(0);
        var p2 = endpointIndex == 0 ? line.GetEndPoint(1) : newPoint;

        return IsTooShort(p1, p2) ? null : Line.CreateBound(p1, p2);
    }

    // Быстрый ключ кривой с квантованием по tolerance
    private string BuildCurveKey(Curve curve) {
        switch (curve) {
            case Line line: {
                var a = line.GetEndPoint(0);
                var b = line.GetEndPoint(1);

                string ka = PointKey(a);
                string kb = PointKey(b);

                // Нормализуем порядок, чтобы AB == BA
                return string.CompareOrdinal(ka, kb) <= 0
                    ? $"L|{ka}|{kb}"
                    : $"L|{kb}|{ka}";
            }
            case Arc arc: {
                string c = PointKey(arc.Center);
                string r = Quantize(arc.Radius).ToString(CultureInfo.InvariantCulture);

                string s = PointKey(arc.GetEndPoint(0));
                string e = PointKey(arc.GetEndPoint(1));

                if (string.CompareOrdinal(s, e) > 0) {
                    (s, e) = (e, s);
                }

                return $"A|{c}|{r}|{s}|{e}";
            }
            default:
                return null;
        }
    }

    private string PointKey(XYZ p) {
        long x = Quantize(p.X);
        long y = Quantize(p.Y);
        long z = Quantize(p.Z);
        return $"{x}:{y}:{z}";
    }

    private long Quantize(double value) {
        if (_tolerance <= 0) return (long)Math.Round(value * 1_000_000.0);
        return (long)Math.Round(value / _tolerance);
    }

    
    private bool IsEqual(Curve curve1, Curve curve2) {
        if (curve1.GetType() != curve2.GetType()) {
            return false;
        }

        switch (curve1) {
            case Line line1 when curve2 is Line line2: {
                var s1 = line1.GetEndPoint(0);
                var e1 = line1.GetEndPoint(1);
                var s2 = line2.GetEndPoint(0);
                var e2 = line2.GetEndPoint(1);
                return (s1.IsAlmostEqualTo(s2) && e1.IsAlmostEqualTo(e2)) ||
                       (s1.IsAlmostEqualTo(e2) && e1.IsAlmostEqualTo(s2));
            }
            case Arc arc1 when curve2 is Arc arc2: {
                return arc1.Center.IsAlmostEqualTo(arc2.Center)
                       && Math.Abs(arc1.Radius - arc2.Radius) < _tolerance
                       && ((arc1.GetEndPoint(0).IsAlmostEqualTo(arc2.GetEndPoint(0))
                            && arc1.GetEndPoint(1).IsAlmostEqualTo(arc2.GetEndPoint(1)))
                           || (arc1.GetEndPoint(0).IsAlmostEqualTo(arc2.GetEndPoint(1))
                               && arc1.GetEndPoint(1).IsAlmostEqualTo(arc2.GetEndPoint(0))));
            }
            default:
                return false;
        }
    }
}
