using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

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

        /// <summary>Измеряет интервалы вдоль отрезка в координатах проекта, включая размещение связи.</summary>
        public IReadOnlyList<MaterialInterval> Intersect(XYZ start, XYZ end) =>
            Geometry.Intersect(Element, FromProject?.OfPoint(start) ?? start, FromProject?.OfPoint(end) ?? end);
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
            var center = opening.Geometry?.SearchTopCenter ?? ReadOptional(() => ReadSlabProbeCenter(geometry, opening));
            if(center is { } origin) {
                columns.Add(new Column(opening, origin));
            } else {
                results.Add(UnknownTop(opening, minimumMm));
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
                var slabs = ReadSlabsAtHead(column, geometry.Document.Application.ShortCurveTolerance);
                if(slabs.Count > 0) {
                    results.Add(new AboveOpeningFacts(column.Opening.Element.UniqueId, column.Origin,
                        new UpperStructureResult(slabs, LengthEpsilon, null, null)));
                    continue;
                }
                if(column.Opening.Geometry is { TopCenter: null, HasConflictingCut: true }) {
                    results.Add(UnknownTop(column.Opening, minimumMm));
                    continue;
                }
                bool incomplete = catalogIncomplete || !linksComplete || unlocated.Any(id => !column.Opening.IsCutWall(id));
                var measured = MeasureColumn(geometry, column, minimumMm, incomplete);
                results.Add(column.Opening.Geometry.TopCenter.HasValue || measured.LimitingStructure is not null
                    ? measured : UnknownTop(column.Opening, minimumMm));
            }
        } finally {
            foreach(var resource in linkResources) {
                resource.Dispose();
            }
        }
        return results.AsReadOnly();
    }

    // Проецирует центральную плоскость экземпляра на середину стены и берёт отметку верха относительно уровня.
    private static GeometryPoint? ReadSlabProbeCenter(IGeometryReadContext geometry, ManagerOpeningSnapshot opening) {
        if(opening.Geometry?.Wall is not { } wall
            || geometry.Document.GetElement(opening.Element.UniqueId) is not FamilyInstance instance
            || geometry.Document.GetElement(instance.LevelId) is not Level level
            || geometry.GetWidthPlane(instance) is not { } width
            || !instance.IsExistsParam(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM)
            || instance.GetParam(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM) is not { HasValue: true } head) {
            return null;
        }
        using var pose = instance.GetTransform();
        if(pose.BasisX.Normalize().CrossProduct(AsXyz(wall.Along)).GetLength() > DirectionEpsilon) {
            return null;
        }
        var center = wall.ToLocal(AsPoint(pose.OfPoint(new XYZ(width.Offset, 0, 0))));
        return wall.ToWorld(center.X, 0, level.ProjectElevation + head.AsDouble() - wall.Origin.Z);
    }

    // Пересекает плиту снизу вверх, чтобы отличить её нижнюю поверхность от точки внутри тела или в отверстии.
    private static List<UpperStructureHit> ReadSlabsAtHead(Column column, double reach) {
        var hits = new List<UpperStructureHit>();
        var head = AsXyz(column.Origin);
        var start = head - XYZ.BasisZ * reach;
        foreach(var (candidate, bounds) in column.Candidates) {
            if(candidate.Category != BuiltInCategory.OST_Floors || bounds.Min.Z > head.Z + LengthEpsilon) {
                continue;
            }
            var end = new XYZ(head.X, head.Y, Math.Max(head.Z + reach, bounds.Max.Z) + LengthEpsilon);
            var intervals = candidate.Intersect(start, end);
            if(intervals is null) {
                continue;
            }
            var contact = intervals.SkipWhile(i => i.End <= reach).ToArray();
            if(contact.Length > 0 && Math.Abs(contact[0].Start - reach) <= LengthEpsilon) {
                var above = contact.Select(i => new MaterialInterval(Math.Max(0, i.Start - reach), i.End - reach));
                hits.Add(new UpperStructureHit(candidate.UniqueId, candidate.TypeId, candidate.Category.ToString(),
                    above.ToArray(), isSlab: true, isLinked: candidate.FromProject is not null));
            }
        }
        return hits;
    }

    // При неподтверждённом верхе сохраняет неизвестную высоту кладки.
    private static AboveOpeningFacts UnknownTop(ManagerOpeningSnapshot opening, double minimumMm) {
        string reason = opening.Geometry?.Limitation ?? "Не определён верхний центр выреза.";
        return new AboveOpeningFacts(opening.Element.UniqueId, null, new CentralMasonry(null, minimumMm, 0, reason),
            new UpperStructureResult([], LengthEpsilon, 0, reason));
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
            if(!column.Opening.IsCutWall(candidate.UniqueId) && bounds.Max.Z > column.Origin.Z) {
                column.Candidates.Add((candidate, bounds));
            }
        }
    }

    // Строит общий конечный отрезок; погрешность сравнения высот ограничена ошибкой мировых координат.
    private AboveOpeningFacts MeasureColumn(IGeometryReadContext geometry, Column column,
        double minimumMm, bool incomplete) {
        var start = AsXyz(column.Origin);
        var walls = column.Opening.CutWallIds.Select(id => geometry.Document.GetElement(id) as Wall).ToArray();
        var boundsByWall = walls.Select(geometry.GetBounds).ToArray();
        double wallTop = boundsByWall.Max(bounds => bounds?.Max.Z) ?? start.Z;
        double top = Math.Max(start.Z, wallTop);
        foreach(var candidate in column.Candidates) {
            top = Math.Max(top, candidate.Bounds.Max.Z);
        }
        var end = ProbeEnd(geometry, start, top);
        var sections = walls.Select((wall, i) => boundsByWall[i] is { } bounds
            ? geometry.Intersect(wall, start, end ?? ProbeEnd(geometry, start, bounds.Max.Z)) : null).ToList();
        var wallIntervals = sections.Count == 0 || sections.Any(part => part is null) ? null
            : sections.Count == 1 ? sections[0] : MaterialInterval.Union(sections.SelectMany(part => part));
        double heightScale = Math.Max(1, Math.Max(Math.Abs(start.Z), Math.Abs(wallTop)));
        double roundoff = Math.Min(LengthEpsilon, SystemPluginConfig.CoordinateRoundoffFactor * heightScale);
        var masonry = new CentralMasonry(wallIntervals, minimumMm, roundoff,
            wallIntervals is null ? "Не удалось измерить интервалы материала стены-хоста." : null);

        var hits = new List<UpperStructureHit>();
        double? unknownFrom = incomplete ? 0 : null;
        string reason = incomplete ? "Часть верхних конструкций не удалось определить или локализовать." : null;
        foreach(var (candidate, bounds) in column.Candidates) {
            var target = end ?? ProbeEnd(geometry, start, bounds.Max.Z);
            var intervals = target is null ? null : candidate.Intersect(start, target);
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
