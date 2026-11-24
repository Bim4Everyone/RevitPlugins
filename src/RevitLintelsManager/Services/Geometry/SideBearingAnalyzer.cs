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
    private sealed class SideProbe(BearingSide side, string hostId, GeometryPoint? start, GeometryPoint? direction,
        ContinuousMaterial masonry, double? ruleMm, double roundoffFeet) {
        public string HostId { get; } = hostId;
        public GeometryPoint? Start { get; } = start;
        public GeometryPoint? Direction { get; } = direction;
        public bool NeedsContact => masonry.LengthMm.HasValue && ruleMm.HasValue
            && masonry.LengthMm.Value + roundoffFeet * SystemPluginConfig.MillimetersPerFoot < ruleMm.Value;
        public GeometryPoint Boundary => AsPoint(AsXyz(Start.Value)
            + AsXyz(Direction.Value) * (masonry.LengthMm.Value / SystemPluginConfig.MillimetersPerFoot));
        public List<BearingIntersection> Hits { get; } = [];

        public SideBearingFacts Finish(bool incomplete) =>
            new(side, Start, Direction, masonry, ruleMm, roundoffFeet, Hits, incomplete);
    }

    /// <summary>Измеряет обе стороны опубликованных проёмов и проверяет тела у концов непрерывной кладки.</summary>
    public IReadOnlyList<OpeningBearingFacts> ReadSideBearings(IGeometryReadContext geometry,
        IReadOnlyList<ManagerOpeningSnapshot> openings,
        IReadOnlyList<StructureCandidate> candidates, bool catalogIncomplete, ManagerConfigSnapshot config) {
        var byType = config.Rules.SelectMany(group => group.WallTypes.Select(type => type.UniqueId).Distinct()
                .Select(typeId => (typeId, group)))
            .ToLookup(entry => entry.typeId, entry => entry.group);
        var rows = new List<(string Id, SideProbe Left, SideProbe Right)>();
        var probes = new List<SideProbe>();
        var hostTypes = new Dictionary<ElementId, string>();
        foreach(var opening in openings) {
            var host = opening.HostWallUniqueId is null ? null
                : ReadOptional(() => geometry.Document.GetElement(opening.HostWallUniqueId) as Wall);
            string type = host is null ? null : ReadOptional(() =>
                GetCachedName(host.GetTypeId(), hostTypes, id => geometry.Document.GetElement(id)?.UniqueId));
            double? ruleMm = type is null ? null : ReadBearingLimit(opening.WidthMm, byType[type]);
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
            var index = new MatchColumns<SideProbe>(probes, probe => probe.Boundary);
            foreach(var candidate in candidates) {
                var bounds = ReadOptional(() => geometry.GetOwnerEnvelope(candidate.Element));
                if(bounds is null) {
                    unlocated.Add(candidate.UniqueId);
                    continue;
                }
                foreach(var probe in index.Within(bounds, LengthEpsilon)) {
                    var boundary = probe.Boundary;
                    if(candidate.UniqueId == probe.HostId
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

        bool Incomplete(string hostId) => catalogIncomplete || unlocated.Count > 1
            || (unlocated.Count == 1 && !unlocated.Contains(hostId));
        return rows.Select(row => new OpeningBearingFacts(row.Id, row.Left.Finish(Incomplete(row.Left.HostId)),
            row.Right.Finish(Incomplete(row.Right.HostId)))).ToList().AsReadOnly();
    }

    // Тип стены задаёт одну группу. Ширина из сопоставленного параметра определяет включённый диапазон.
    private static double? ReadBearingLimit(double? widthMm, IEnumerable<PlacementRuleSnapshot> groups) {
        if(widthMm is null or <= 0) {
            return null;
        }
        double width = Math.Round(widthMm.Value, MidpointRounding.AwayFromZero);
        var eligible = groups.ToList();
        if(eligible.Count != 1) {
            return null;
        }
        var ranges = eligible[0].Rules.Where(rule => width >= rule.MinWidth && width <= rule.MaxWidth).ToList();
        return ranges.Count == 1 && ranges[0].WallSupport > 0 ? ranges[0].WallSupport : null;
    }

    // Начало на верхней кромке фактического выреза; направление не зависит от отражения вставленного семейства.
    private SideProbe MeasureSide(IGeometryReadContext geometry, ManagerOpeningSnapshot opening, Wall host,
        BearingSide side, double minimumMm, double? ruleMm) {
        var cut = opening.Geometry?.Cut;
        var frame = opening.Geometry?.Wall;
        if(cut is null || frame is null) {
            string reason = opening.Geometry?.Limitation ?? "Не определена кромка выреза или ось стены.";
            var missing = new ContinuousMaterial(null, minimumMm, 0, reason);
            return new SideProbe(side, opening.HostWallUniqueId, null, null, missing, ruleMm, 0);
        }
        var direction = AsXyz(frame.Along) * (side == BearingSide.Left ? -1 : 1);
        var start = AsXyz(cut.TopCenter) + direction * (cut.Width / 2);
        var bounds = geometry.GetBounds(host);
        double scale = Math.Max(1, Math.Max(Math.Abs(start.X), Math.Abs(start.Y)));
        if(bounds is not null) {
            scale = Math.Max(scale, Math.Max(Math.Max(Math.Abs(bounds.Min.X), Math.Abs(bounds.Max.X)),
                Math.Max(Math.Abs(bounds.Min.Y), Math.Abs(bounds.Max.Y))));
        }
        double roundoff = Math.Min(LengthEpsilon, SystemPluginConfig.CoordinateRoundoffFactor * scale);
        var intervals = ReadSideIntervals(geometry, host, bounds, AsPoint(start), AsPoint(direction));
        var masonry = new ContinuousMaterial(intervals, minimumMm, roundoff,
            intervals is null ? "Не удалось измерить тело стены от кромки выреза." : null);
        return new SideProbe(side, opening.HostWallUniqueId, AsPoint(start), AsPoint(direction),
            masonry, ruleMm, roundoff);
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
