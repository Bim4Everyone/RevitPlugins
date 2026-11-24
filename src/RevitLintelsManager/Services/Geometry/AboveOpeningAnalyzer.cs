using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitLintelsManager.Models.Configs;

using static RevitLintelsManager.Models.RevitElementRead;
using static RevitLintelsManager.Models.RevitGeometry;

namespace RevitLintelsManager.Models;

/// <summary>Измеряет кладку и конструкции над верхним центром проёма.</summary>
internal sealed class AboveOpeningAnalyzer : IAboveOpeningAnalyzer {
    private sealed class Column(ManagerOpeningSnapshot opening) {
        public ManagerOpeningSnapshot Opening { get; } = opening;
        public GeometryPoint Origin => Opening.Geometry.Cut.TopCenter;
        public List<(StructureCandidate Candidate, GeometryBounds Bounds)> Candidates { get; } = [];
    }

    /// <summary>Измеряет материал на центральных вертикалях проёмов.</summary>
    public IReadOnlyList<AboveOpeningFacts> ReadAboveOpenings(IGeometryReadContext geometry,
        IReadOnlyList<ManagerOpeningSnapshot> openings,
        IReadOnlyList<StructureCandidate> candidates, bool catalogIncomplete, double minimumMm) {
        var results = new List<AboveOpeningFacts>(openings.Count);
        var columns = new List<Column>();
        foreach(var opening in openings) {
            if(opening.Geometry?.Cut is not null) {
                columns.Add(new Column(opening));
            } else {
                string reason = opening.Geometry?.Limitation ?? "Не определён верхний центр выреза.";
                results.Add(new AboveOpeningFacts(opening.Element.UniqueId, null,
                    new CentralMasonry(null, minimumMm, 0, reason),
                    new UpperStructureResult([], LengthEpsilon, 0, reason)));
            }
        }
        if(columns.Count == 0) {
            return results.AsReadOnly();
        }

        var index = new MatchColumns<Column>(columns, c => c.Origin);
        var unlocated = new HashSet<string>();
        foreach(var candidate in candidates) {
            var bounds = ReadOptional(() => geometry.GetOwnerEnvelope(candidate.Element));
            if(bounds is null) {
                unlocated.Add(candidate.UniqueId);
                continue;
            }
            foreach(var column in index.Within(bounds, LengthEpsilon)) {
                if(candidate.UniqueId != column.Opening.HostWallUniqueId && bounds.Max.Z > column.Origin.Z) {
                    column.Candidates.Add((candidate, bounds));
                }
            }
        }
        foreach(var column in columns) {
            bool incomplete = catalogIncomplete || unlocated.Count > 1
                || (unlocated.Count == 1 && !unlocated.Contains(column.Opening.HostWallUniqueId));
            results.Add(MeasureColumn(geometry, column, minimumMm, incomplete));
        }
        return results.AsReadOnly();
    }

    // Строит общий конечный отрезок; погрешность сравнения высот ограничена ошибкой мировых координат.
    private AboveOpeningFacts MeasureColumn(IGeometryReadContext geometry, Column column,
        double minimumMm, bool incomplete) {
        var start = AsXyz(column.Origin);
        string hostId = column.Opening.HostWallUniqueId;
        var host = hostId is null ? null : ReadOptional(() => geometry.Document.GetElement(hostId) as Wall);
        var hostBounds = geometry.GetBounds(host);
        double top = Math.Max(start.Z, hostBounds?.Max.Z ?? start.Z);
        foreach(var candidate in column.Candidates) {
            top = Math.Max(top, candidate.Bounds.Max.Z);
        }
        var end = ProbeEnd(geometry, start, top);
        var wallIntervals = hostBounds is null ? null
            : geometry.Intersect(host, start, end ?? ProbeEnd(geometry, start, hostBounds.Max.Z));
        double heightScale = Math.Max(1, Math.Max(Math.Abs(start.Z), Math.Abs(hostBounds?.Max.Z ?? start.Z)));
        double roundoff = Math.Min(LengthEpsilon, SystemPluginConfig.CoordinateRoundoffFactor * heightScale);
        var masonry = new CentralMasonry(wallIntervals, minimumMm, roundoff,
            wallIntervals is null ? "Не удалось измерить интервалы материала стены-хоста." : null);

        var hits = new List<UpperStructureHit>();
        double? unknownFrom = incomplete ? 0 : null;
        string reason = incomplete ? "Часть верхних конструкций не удалось определить или локализовать." : null;
        foreach(var (candidate, bounds) in column.Candidates) {
            var intervals = geometry.Intersect(candidate.Element, start,
                end ?? ProbeEnd(geometry, start, bounds.Max.Z));
            if(intervals is null) {
                double lower = Math.Max(0, bounds.Min.Z - start.Z);
                if(!unknownFrom.HasValue || lower < unknownFrom.Value) {
                    unknownFrom = lower;
                    reason = "Не удалось измерить тело конструкции " + candidate.UniqueId;
                }
            } else if(intervals.Count > 0) {
                hits.Add(new UpperStructureHit(candidate.UniqueId, candidate.TypeId,
                    candidate.Category.ToString(), intervals));
            }
        }
        return new AboveOpeningFacts(column.Opening.Element.UniqueId, column.Origin, masonry,
            new UpperStructureResult(hits, LengthEpsilon, unknownFrom, reason));
    }

    // Минимальная длина нужна только для создания допустимой Curve, даже при очень тонком материале.
    private XYZ ProbeEnd(IGeometryReadContext geometry, XYZ start, double top) {
        double z = Math.Max(top, start.Z + geometry.Document.Application.ShortCurveTolerance) + LengthEpsilon;
        var end = new XYZ(start.X, start.Y, z);
        return XYZ.IsWithinLengthLimits(end) ? end : null;
    }
}
