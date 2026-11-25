using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitLintelsManager.Models.Configs;

using static RevitLintelsManager.Models.RevitElementRead;
using static RevitLintelsManager.Models.RevitGeometry;

namespace RevitLintelsManager.Models;

/// <summary>Измеряет кладку и конструкции над верхним центром проёма.</summary>
internal sealed class AboveOpeningAnalyzer(Func<Document, IGeometryReadContext> openGeometry) : IAboveOpeningAnalyzer {
    private sealed class Candidate(Element element, string uniqueId, string typeId, BuiltInCategory category,
        IGeometryReadContext geometry, Transform fromProject = null) {
        public Element Element { get; } = element;
        public string UniqueId { get; } = uniqueId;
        public string TypeId { get; } = typeId;
        public BuiltInCategory Category { get; } = category;
        public IGeometryReadContext Geometry { get; } = geometry;
        public Transform FromProject { get; } = fromProject;
    }

    private sealed class Column(ManagerOpeningSnapshot opening, GeometryPoint origin) {
        public ManagerOpeningSnapshot Opening { get; } = opening;
        public GeometryPoint Origin { get; } = origin;
        public List<(Candidate Candidate, GeometryBounds Bounds)> Candidates { get; } = [];
    }

    /// <summary>Измеряет материал на центральных вертикалях проёмов.</summary>
    public IReadOnlyList<AboveOpeningFacts> ReadAboveOpenings(IGeometryReadContext geometry,
        IReadOnlyList<ManagerOpeningSnapshot> openings,
        IReadOnlyList<StructureCandidate> candidates, bool catalogIncomplete, double minimumMm, ElementId phaseId) {
        var results = new List<AboveOpeningFacts>(openings.Count);
        var columns = new List<Column>();
        foreach(var opening in openings) {
            if(opening.Geometry?.TopCenter is { } origin) {
                columns.Add(new Column(opening, origin));
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
            var source = new Candidate(candidate.Element, candidate.UniqueId, candidate.TypeId, candidate.Category,
                geometry);
            AddCandidate(index, unlocated, source, ReadOptional(() => geometry.GetOwnerEnvelope(candidate.Element)));
        }

        var linkResources = new List<IDisposable>();
        try {
            bool linksComplete = AddLinkedSlabs(geometry.Document, phaseId, index, unlocated, linkResources);
            foreach(var column in columns) {
                bool incomplete = catalogIncomplete || !linksComplete || unlocated.Count > 1
                    || (unlocated.Count == 1 && !unlocated.Contains(column.Opening.CutWallUniqueId));
                results.Add(MeasureColumn(geometry, column, minimumMm, incomplete));
            }
        } finally {
            foreach(var resource in linkResources) {
                resource.Dispose();
            }
        }
        return results.AsReadOnly();
    }

    // Использует фазу связи и её полное преобразование только для геометрии плит над проёмом.
    private bool AddLinkedSlabs(Document document, ElementId phaseId, MatchColumns<Column> index,
        ISet<string> unlocated, ICollection<IDisposable> resources) {
        var sources = new Dictionary<Document, IGeometryReadContext>();
        using var links = new FilteredElementCollector(document).OfClass(typeof(RevitLinkInstance));
        bool complete = true;
        foreach(var link in links.Cast<RevitLinkInstance>()) {
            try {
                var linked = link.GetLinkDocument();
                if(linked is null || document.GetElement(link.GetTypeId()) is not RevitLinkType type
                    || !type.GetPhaseMap().TryGetValue(phaseId, out var linkedPhase)) {
                    complete = false;
                    continue;
                }
                using var toProject = link.GetTotalTransform();
                var fromProject = toProject.Inverse;
                resources.Add(fromProject);
                if(!sources.TryGetValue(linked, out var geometry)) {
                    geometry = openGeometry(linked);
                    sources.Add(linked, geometry);
                    resources.Add(geometry);
                }
                using var floors = new FilteredElementCollector(linked).OfCategory(BuiltInCategory.OST_Floors)
                    .WhereElementIsNotElementType();
                foreach(var floor in floors) {
                    if(floor.GetPhaseStatus(linkedPhase) is not (ElementOnPhaseStatus.New
                        or ElementOnPhaseStatus.Existing)) {
                        continue;
                    }
                    string id = $"{link.UniqueId}/{floor.UniqueId}";
                    string floorType = linked.GetElement(floor.GetTypeId())?.UniqueId;
                    var source = new Candidate(floor, id, floorType, BuiltInCategory.OST_Floors, geometry, fromProject);
                    AddCandidate(index, unlocated, source, ToProjectBounds(geometry.GetBounds(floor), toProject));
                }
            } catch(Exception exception) when(IsElementReadException(exception)) {
                complete = false;
            }
        }
        return complete;
    }

    // Ограничивает поиск проекцией рамки, включая поворот и смещение связанной модели.
    private static GeometryBounds ToProjectBounds(GeometryBounds bounds, Transform toProject) {
        if(bounds is null) {
            return null;
        }
        using var box = new BoundingBoxXYZ { Min = AsXyz(bounds.Min), Max = AsXyz(bounds.Max) };
        var points = GetWorldCorners(box).Select(toProject.OfPoint).ToArray();
        return new GeometryBounds(new GeometryPoint(points.Min(p => p.X), points.Min(p => p.Y), points.Min(p => p.Z)),
            new GeometryPoint(points.Max(p => p.X), points.Max(p => p.Y), points.Max(p => p.Z)));
    }

    // Добавляет проверяемый элемент только к вертикалям, пересекающим его рамку над верхом проёма.
    private static void AddCandidate(MatchColumns<Column> index, ISet<string> unlocated, Candidate candidate,
        GeometryBounds bounds) {
        if(bounds is null) {
            unlocated.Add(candidate.UniqueId);
            return;
        }
        foreach(var column in index.Within(bounds, LengthEpsilon)) {
            if(candidate.UniqueId != column.Opening.CutWallUniqueId && bounds.Max.Z > column.Origin.Z) {
                column.Candidates.Add((candidate, bounds));
            }
        }
    }

    // Строит общий конечный отрезок; погрешность сравнения высот ограничена ошибкой мировых координат.
    private AboveOpeningFacts MeasureColumn(IGeometryReadContext geometry, Column column,
        double minimumMm, bool incomplete) {
        var start = AsXyz(column.Origin);
        string hostId = column.Opening.CutWallUniqueId;
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
            var target = end ?? ProbeEnd(geometry, start, bounds.Max.Z);
            var intervals = target is null ? null : candidate.Geometry.Intersect(candidate.Element,
                candidate.FromProject?.OfPoint(start) ?? start, candidate.FromProject?.OfPoint(target) ?? target);
            if(intervals is null) {
                double lower = Math.Max(0, bounds.Min.Z - start.Z);
                if(!unknownFrom.HasValue || lower < unknownFrom.Value) {
                    unknownFrom = lower;
                    reason = "Не удалось измерить тело конструкции " + candidate.UniqueId;
                }
            } else if(intervals.Count > 0) {
                hits.Add(new UpperStructureHit(candidate.UniqueId, candidate.TypeId,
                    candidate.Category.ToString(), intervals, candidate.Category == BuiltInCategory.OST_Floors,
                    candidate.FromProject is not null));
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
