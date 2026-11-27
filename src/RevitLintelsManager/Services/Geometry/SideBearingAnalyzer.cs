using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitLintelsManager.Models.Configs;

using static RevitLintelsManager.Models.RevitElementRead;
using static RevitLintelsManager.Models.RevitGeometry;

namespace RevitLintelsManager.Models;

/// <summary>Измеряет кладку и контакты у физических боковых границ проёма.</summary>
internal sealed class SideBearingAnalyzer : ISideBearingAnalyzer {
    private sealed class EndProbe(OpeningLintelMatch match, RetainedLintelEnds end,
        GeometryPoint start, GeometryPoint direction, double bearingFeet) {
        public OpeningLintelMatch Match { get; } = match;
        public RetainedLintelEnds End { get; } = end;
        public GeometryPoint Start { get; } = start;
        public GeometryPoint Direction { get; } = direction;
        public double BearingFeet { get; } = bearingFeet;
        public GeometryPoint Point { get; } = AsPoint(AsXyz(start) + AsXyz(direction) * bearingFeet);
        public bool InOrdinaryWall { get; set; }
        public bool InConcreteWall { get; set; }
    }

    private sealed class SideProbe(BearingSide side, ManagerOpeningSnapshot opening, GeometryPoint? start, GeometryPoint? direction,
        ContinuousMaterial masonry, double? ruleMm, double roundoffFeet) {
        private readonly List<MaterialInterval> _adjoiningIntervals = [];
        private ContinuousMaterial _masonry = masonry;
        private bool _incomplete;

        public ManagerOpeningSnapshot Opening { get; } = opening;
        public GeometryPoint? Start { get; } = start;
        public GeometryPoint? Direction { get; } = direction;
        public bool NeedsContact => _masonry.LengthMm.HasValue && ruleMm.HasValue
            && _masonry.LengthMm.Value + roundoffFeet * SystemPluginConfig.MillimetersPerFoot < ruleMm.Value;
        public double RuleFeet => ruleMm.Value / SystemPluginConfig.MillimetersPerFoot;
        public GeometryPoint RuleEnd => AsPoint(AsXyz(Start.Value) + AsXyz(Direction.Value) * RuleFeet);
        public GeometryPoint Boundary => AsPoint(AsXyz(Start.Value)
            + AsXyz(Direction.Value) * _masonry.LengthFeet.Value);
        public List<BearingIntersection> Hits { get; } = [];

        public void AddAdjoiningIntervals(IReadOnlyList<MaterialInterval> intervals) {
            if(intervals is null) {
                _incomplete = true;
            } else {
                _adjoiningIntervals.AddRange(intervals);
            }
        }

        public void MergeMasonry() {
            if(_adjoiningIntervals.Count > 0) {
                var intervals = MaterialInterval.Union(_masonry.Intervals.Concat(_adjoiningIntervals));
                _masonry = new ContinuousMaterial(intervals, _masonry.MinimumMm, roundoffFeet);
            }
        }

        public SideBearingFacts Finish(bool incomplete) =>
            new(side, Start, Direction, _masonry, ruleMm, roundoffFeet, Hits, incomplete || _incomplete);
    }

    /// <summary>Измеряет обе стороны опубликованных проёмов и проверяет тела у концов непрерывной кладки.</summary>
    public IReadOnlyList<OpeningBearingFacts> ReadSideBearings(IGeometryReadContext geometry,
        IReadOnlyList<ManagerOpeningSnapshot> openings,
        IReadOnlyList<StructureCandidate> candidates, bool catalogIncomplete, ManagerConfigSnapshot config) {
        var rules = PlacementRuleLookup.For(config);
        var rows = new List<(string Id, SideProbe Left, SideProbe Right)>();
        var probes = new List<SideProbe>();
        var hostTypes = new Dictionary<ElementId, string>();
        foreach(var opening in openings) {
            var host = opening.CutWallUniqueId is null ? null
                : ReadOptional(() => geometry.Document.GetElement(opening.CutWallUniqueId) as Wall);
            string type = host is null ? null : ReadOptional(() =>
                GetCachedName(host.GetTypeId(), hostTypes, id => geometry.Document.GetElement(id)?.UniqueId));
            double? ruleMm = rules.Resolve(type, opening.WidthMm).Rule?.WallSupport;
            var left = MeasureSide(geometry, opening, host, BearingSide.Left, config.MinimalBearingMm, ruleMm);
            var right = MeasureSide(geometry, opening, host, BearingSide.Right, config.MinimalBearingMm, ruleMm);
            rows.Add((opening.Element.UniqueId, left, right));
            if(left.NeedsContact) {
                probes.Add(left);
            }
            if(right.NeedsContact) {
                probes.Add(right);
            }
        }

        var unlocated = new HashSet<string>();
        if(probes.Count > 0) {
            AddAdjoiningMasonry(geometry, probes, candidates, unlocated);
            var index = new MatchColumns<SideProbe>(probes.Where(probe => probe.NeedsContact), probe => probe.Boundary);
            foreach(var candidate in candidates.Where(candidate => !candidate.Masonry)) {
                var bounds = ReadOptional(() => geometry.GetOwnerEnvelope(candidate.Element));
                if(bounds is null) {
                    unlocated.Add(candidate.UniqueId);
                    continue;
                }
                foreach(var probe in index.Within(bounds, LengthEpsilon)) {
                    var boundary = probe.Boundary;
                    if(probe.Opening.IsCutWall(candidate.UniqueId)
                        || !bounds.Overlaps(boundary, boundary, LengthEpsilon)) {
                        continue;
                    }
                    var intervals = ReadSideIntervals(geometry, candidate.Element, bounds,
                        probe.Start.Value, probe.Direction.Value);
                    probe.Hits.Add(new BearingIntersection(candidate.UniqueId, candidate.TypeId,
                        candidate.Category.ToString(), candidate.Concrete, candidate.Steel, intervals));
                }
            }
        }

        return rows.Select(row => {
            bool incomplete = catalogIncomplete || unlocated.Any(id => !row.Left.Opening.IsCutWall(id));
            return new OpeningBearingFacts(row.Id, row.Left.Finish(incomplete), row.Right.Finish(incomplete));
        }).ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public IReadOnlyList<OpeningLintelMatch> ReadRetainedEnds(IGeometryReadContext geometry,
        IReadOnlyList<OpeningLintelMatch> matches, IReadOnlyList<EndWallSnapshot> walls, string familyId, int? leftSign) {
        if(leftSign is not (-1 or 1) || walls.Count == 0) {
            return matches;
        }
        var probes = matches.SelectMany(match => GetFreeEndProbes(geometry, match, familyId, leftSign.Value)).ToList();
        if(probes.Count == 0) {
            return matches;
        }
        var index = new MatchColumns<EndProbe>(probes, probe => probe.Point);
        foreach(var wall in walls) {
            var bounds = wall.Bounds;
            if(bounds is null) {
                continue;
            }
            foreach(var probe in index.Within(bounds, LengthEpsilon)) {
                var point = probe.Point;
                if(probe.Match.Opening.IsCutWall(wall.UniqueId) || probe.InConcreteWall
                    || (probe.InOrdinaryWall && !wall.Concrete) || !bounds.Overlaps(point, point, LengthEpsilon)) {
                    continue;
                }
                var intervals = ReadSideIntervals(geometry, geometry.Document.GetElement(wall.UniqueId), bounds,
                    probe.Start, probe.Direction);
                if(intervals?.Any(interval => interval.Start + LengthEpsilon < probe.BearingFeet
                    && interval.End + LengthEpsilon >= probe.BearingFeet) == true) {
                    probe.InConcreteWall = wall.Concrete;
                    probe.InOrdinaryWall |= !wall.Concrete;
                }
            }
        }

        var retained = probes.Where(probe => probe.InOrdinaryWall && !probe.InConcreteWall)
            .GroupBy(probe => probe.Match)
            .ToDictionary(group => group.Key, group => group.Aggregate(RetainedLintelEnds.None,
                (ends, probe) => ends | probe.End));
        return matches.Select(match => retained.TryGetValue(match, out var ends)
            ? new OpeningLintelMatch(match, ends) : match).ToList().AsReadOnly();
    }

    // Строит отрезки от фактических кромок пролёта до концов, выходящих за торец стены-хоста.
    private IEnumerable<EndProbe> GetFreeEndProbes(IGeometryReadContext geometry, OpeningLintelMatch match,
        string familyId, int leftSign) {
        var lintel = match.Lintel;
        if(match.State is not (MatchState.Confirmed or MatchState.Shifted)
            || lintel?.Element.FamilyUniqueId != familyId
            || lintel.Geometry?.Anchor is not { } anchor
            || match.Opening?.Geometry?.Cut?.TargetPlanes is not { } planes
            || lintel.OpeningWidthMm is not double width || !CalculationNumbers.IsFinite(width) || width <= 0) {
            yield break;
        }
        var target = new LintelTarget(planes, leftSign);
        if(target.GetPhysicalLeft(lintel.Geometry.Placement) is null) {
            yield break;
        }
        var host = ReadOptional(() => geometry.Document.GetElement(match.Opening.CutWallUniqueId) as Wall);
        var bounds = geometry.GetBounds(host);
        if(bounds is null) {
            yield break;
        }
        var left = AsXyz(anchor.CenterNormal) * leftSign;
        var center = AsXyz(anchor.Center);
        foreach(var (end, bearing, axis) in new[] {
            (RetainedLintelEnds.Left, lintel.LeftBearingMm, left),
            (RetainedLintelEnds.Right, lintel.RightBearingMm, -left)
        }) {
            if(bearing is not double mm || !CalculationNumbers.IsFinite(mm) || mm <= 0) {
                continue;
            }
            double length = mm / SystemPluginConfig.MillimetersPerFoot;
            var start = AsPoint(center + axis * (width / (2 * SystemPluginConfig.MillimetersPerFoot)));
            var direction = AsPoint(axis);
            var hostIntervals = ReadCutWallIntervals(geometry, match.Opening, host, bounds, start, direction);
            if(hostIntervals?.All(interval => interval.End + LengthEpsilon < length) == true) {
                yield return new EndProbe(match, end, start, direction, length);
            }
        }
    }

    // Собирает пересечения соседних стен по всей длине требуемого опирания до проверки креплений.
    private void AddAdjoiningMasonry(IGeometryReadContext geometry, IReadOnlyList<SideProbe> probes,
        IReadOnlyList<StructureCandidate> candidates, ISet<string> unlocated) {
        var index = new MatchColumns<SideProbe>(probes, probe => probe.Start.Value);
        double margin = probes.Max(probe => probe.RuleFeet) + LengthEpsilon;
        foreach(var candidate in candidates.Where(candidate => candidate.Masonry)) {
            var bounds = ReadOptional(() => geometry.GetOwnerEnvelope(candidate.Element));
            if(bounds is null) {
                unlocated.Add(candidate.UniqueId);
                continue;
            }
            foreach(var probe in index.Within(bounds, margin)) {
                if(probe.Opening.IsCutWall(candidate.UniqueId) || !bounds.Overlaps(probe.Start.Value, probe.RuleEnd,
                    LengthEpsilon)) {
                    continue;
                }
                probe.AddAdjoiningIntervals(ReadSideIntervals(geometry, candidate.Element, bounds,
                    probe.Start.Value, probe.Direction.Value));
            }
        }
        foreach(var probe in probes) {
            probe.MergeMasonry();
        }
    }

    // Начало на верхней кромке фактического выреза; направление не зависит от отражения вставленного семейства.
    private SideProbe MeasureSide(IGeometryReadContext geometry, ManagerOpeningSnapshot opening, Wall host,
        BearingSide side, double minimumMm, double? ruleMm) {
        var cut = opening.Geometry?.Cut;
        var frame = opening.Geometry?.Wall;
        var top = opening.Geometry?.OpenTop;
        if((cut is null && !top.HasValue) || frame is null) {
            string reason = opening.Geometry?.Limitation ?? "Не определена кромка выреза или ось стены.";
            var missing = new ContinuousMaterial(null, minimumMm, 0, reason);
            return new SideProbe(side, opening, null, null, missing, ruleMm, 0);
        }
        var direction = AsXyz(frame.Along) * (side == BearingSide.Left ? -1 : 1);
        double width = cut?.Width ?? top.Value.Width;
        var start = AsXyz(cut?.TopCenter ?? top.Value.Center) + direction * (width / 2);
        if(top is { } openTop) {
            // Проверяем откос чуть ниже открытого верха, внутри материала, а не вдоль верхней поверхности.
            start -= XYZ.BasisZ * Math.Min(openTop.JambHeight / 2, geometry.Document.Application.ShortCurveTolerance);
        }
        var bounds = geometry.GetBounds(host);
        double scale = Math.Max(1, Math.Max(Math.Abs(start.X), Math.Abs(start.Y)));
        if(bounds is not null) {
            scale = Math.Max(scale, Math.Max(Math.Max(Math.Abs(bounds.Min.X), Math.Abs(bounds.Max.X)),
                Math.Max(Math.Abs(bounds.Min.Y), Math.Abs(bounds.Max.Y))));
        }
        double roundoff = Math.Min(LengthEpsilon, SystemPluginConfig.CoordinateRoundoffFactor * scale);
        var intervals = ReadCutWallIntervals(geometry, opening, host, bounds, AsPoint(start), AsPoint(direction));
        var masonry = new ContinuousMaterial(intervals, minimumMm, roundoff,
            intervals is null ? "Не удалось измерить тело стены от кромки выреза." : null);
        return new SideProbe(side, opening, AsPoint(start), AsPoint(direction),
            masonry, ruleMm, roundoff);
    }

    // Учитывает материал каждой части стены у общей кромки проёма.
    private IReadOnlyList<MaterialInterval> ReadCutWallIntervals(IGeometryReadContext geometry, ManagerOpeningSnapshot opening,
        Wall host, GeometryBounds bounds, GeometryPoint start, GeometryPoint direction) {
        var intervals = ReadSideIntervals(geometry, host, bounds, start, direction);
        if(intervals is null || opening.Geometry.AdditionalCutWallIds.Count == 0) {
            return intervals;
        }
        var all = new List<MaterialInterval>(intervals);
        foreach(string id in opening.Geometry.AdditionalCutWallIds) {
            var wall = geometry.Document.GetElement(id) as Wall;
            var part = ReadSideIntervals(geometry, wall, geometry.GetBounds(wall), start, direction);
            if(part is null) {
                return null;
            }
            all.AddRange(part);
        }
        return MaterialInterval.Union(all);
    }

    // Дальняя проекция рамки ограничивает конечный отрезок. В длину материала входит только внутренность Solid.
    private IReadOnlyList<MaterialInterval> ReadSideIntervals(IGeometryReadContext geometry, Element element,
        GeometryBounds bounds, GeometryPoint origin, GeometryPoint axis) {
        if(bounds is null) {
            return null;
        }
        double x = axis.X >= 0 ? bounds.Max.X : bounds.Min.X;
        double y = axis.Y >= 0 ? bounds.Max.Y : bounds.Min.Y;
        double far = (x - origin.X) * axis.X + (y - origin.Y) * axis.Y;
        var start = AsXyz(origin);
        double length = Math.Max(geometry.Document.Application.ShortCurveTolerance, far) + LengthEpsilon;
        return geometry.Intersect(element, start, start + AsXyz(axis) * length);
    }
}
