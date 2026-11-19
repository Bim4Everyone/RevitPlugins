using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.RoomDimensions.Geometry;

namespace RevitDocumenter.Models.RoomDimensions.Contours;

/// <summary>
/// Объединяет прямолинейные контуры и подтверждённые полосы игнорируемых стен.
/// Разбивает отрезки на пересечениях и оставляет границу объединения с областью слева.
/// </summary>
internal class AreaBoundaryUnion {
    private const double _tolerance = 1e-7;
    private const double _angleTolerance = 1e-6;
    // GetBoundarySegments может оставлять микростыки между стеной и семейством.
    // Соединяем только такие стыки, не меняя опорные прямые реальных граней.
    private const double _maxInputGap = 0.1 / 304.8;
    private const double _maxCodirectionalInputGap = 1.0 / 304.8;
    private const double _unsourcedJointAngleTolerance = 1e-3;
    private const int _maxSegments = 2000;

    public List<List<ContourLine>> Build(
        List<RoomContour> rooms,
        List<List<Point2D>> bridges,
        HashSet<ElementId> ignoredIds) {
        rooms = CloseMicroJoints(rooms);
        var originals = rooms.SelectMany(r => r.Loops).SelectMany(l => l.Lines).ToList();
        if(originals.Any(l => l.IsArc)) {
            throw new InvalidOperationException("Общая область: дуги контура пока не поддерживаются");
        }
        var edges = originals.ToList();
        foreach(var bridge in bridges) {
            for(int i = 0; i < bridge.Count; i++) {
                var segment = new Segment2D(bridge[i], bridge[(i + 1) % bridge.Count]);
                if(segment.Length > _tolerance) {
                    edges.Add(new ContourLine { Segment = segment, Z = originals[0].Z });
                }
            }
        }
        if(edges.Count > _maxSegments) {
            throw new InvalidOperationException("Общая область: превышен предел 2000 исходных отрезков");
        }

        // Реальные внутренние контуры нельзя заполнить полосой исключённой стены.
        var retainedHoles = rooms.SelectMany(r => r.Loops).Where(l => !l.IsOuter)
            .Where(l => l.Lines.Any(e => e.SourceId is null || !ignoredIds.Contains(e.SourceId)))
            .Select(l => l.Lines.Select(e => e.Segment.Start).ToList()).ToList();
        bool Contains(Point2D point) => (rooms.Any(r => ContainsRoom(r, point))
                                         || bridges.Any(b => ContainsPolygon(b, point)))
                                        && !retainedHoles.Any(h => ContainsPolygon(h, point));
        var boundary = new List<ContourLine>();
        foreach(var edge in edges) {
            var cuts = new List<double> { 0, edge.Segment.Length };
            foreach(var other in edges) {
                AddCuts(edge.Segment, other.Segment, cuts);
            }
            var ordered = cuts.OrderBy(v => v).ToList();
            for(int i = 0; i + 1 < ordered.Count; i++) {
                double length = ordered[i + 1] - ordered[i];
                if(length <= _tolerance) {
                    continue;
                }
                var direction = edge.Segment.Direction;
                var start = edge.Segment.Start + direction * ordered[i];
                var end = edge.Segment.Start + direction * ordered[i + 1];
                var middle = (start + end) * 0.5;
                // На узком стыке границы комнаты и полосы стены проба не должна
                // перескакивать через соседнее ребро, иначе теряется часть контура.
                var probe = direction.PerpendicularLeft() * Math.Min(2 * _tolerance, length * 0.1);
                bool left = Contains(middle + probe);
                bool right = Contains(middle - probe);
                if(left == right) {
                    continue;
                }
                var segment = left ? new Segment2D(start, end) : new Segment2D(end, start);
                var source = FindSource(edge, segment, originals, ignoredIds);
                var piece = Copy(source, segment);
                piece.Z = edge.Z;
                var duplicate = boundary.FirstOrDefault(l => Same(l.Segment.Start, segment.Start)
                                                              && Same(l.Segment.End, segment.End));
                if(duplicate is null) {
                    boundary.Add(piece);
                } else if(!HasSource(duplicate) && HasSource(piece)) {
                    boundary[boundary.IndexOf(duplicate)] = piece;
                }
            }
        }
        return TraceLoops(boundary);
    }

    /// <summary>
    /// Сохраняет оба конца каждого реального отрезка. Короткий соединитель без
    /// Reference делает полигон проверки и набор рёбер согласованными.
    /// Исходные снимки помещений не изменяются, крупные разрывы не замыкаются.
    /// </summary>
    private static List<RoomContour> CloseMicroJoints(List<RoomContour> rooms) {
        var result = new List<RoomContour>();
        foreach(var room in rooms) {
            var copy = new RoomContour(room.RoomType) { BoundaryLocation = room.BoundaryLocation };
            foreach(var loop in room.Loops) {
                var lines = loop.Lines.ToList();
                var closed = new List<ContourElement>();
                for(int i = 0; i < lines.Count; i++) {
                    var line = lines[i];
                    var next = lines[(i + 1) % lines.Count];
                    closed.Add(line);
                    double gap = line.Segment.End.DistanceTo(next.Segment.Start);
                    if(gap <= _tolerance) {
                        continue;
                    }
                    double maxGap = GetInputGapLimit(line, next);
                    if(gap > maxGap) {
                        throw new InvalidOperationException(
                            $"Общая область: разрыв исходного контура "
                            + $"{UnitUtilsHelper.ConvertFromInternalValue(gap):0.#########} мм превышает "
                            + $"{UnitUtilsHelper.ConvertFromInternalValue(maxGap):0.#} мм");
                    }
                    closed.Add(new ContourLine {
                        Segment = new Segment2D(line.Segment.End, next.Segment.Start),
                        Z = line.Z,
                        SourceId = ElementId.InvalidElementId
                    });
                }
                copy.Loops.Add(new ContourLoop {
                    Index = loop.Index,
                    IsOuter = loop.IsOuter,
                    SignedArea = loop.SignedArea,
                    Elements = closed
                });
            }
            result.Add(copy);
        }
        return result;
    }

    private static double GetInputGapLimit(ContourLine line, ContourLine next) {
        if(line.IsArc || next.IsArc) {
            return _maxInputGap;
        }
        // Вспомогательный отрезок без источника может быть чуть наклонён к грани.
        double angleTolerance = HasSource(line) && HasSource(next)
            ? _angleTolerance : _unsourcedJointAngleTolerance;
        bool codirectional = line.Segment.IsParallelTo(next.Segment, angleTolerance)
            && line.Segment.Direction.Dot(next.Segment.Direction) > 0;
        return codirectional ? _maxCodirectionalInputGap : _maxInputGap;
    }

    private static ContourLine FindSource(
        ContourLine edge, Segment2D segment, List<ContourLine> originals, HashSet<ElementId> ignoredIds) {
        if(HasSource(edge) && !ignoredIds.Contains(edge.SourceId)) {
            return edge;
        }
        // Торец полосы может продолжать одну и ту же сохранённую стену по обе стороны.
        // Не используем плоскость произвольной соседней стены и исключённые источники.
        var candidates = originals.Where(l => HasSource(l) && !ignoredIds.Contains(l.SourceId)
            && l.Segment.IsParallelTo(segment, _angleTolerance)
            && Math.Abs((l.Segment.Start - segment.Start).Cross(segment.Direction)) < _tolerance).ToList();
        foreach(var group in candidates.GroupBy(l => l.SourceId)) {
            bool before = group.Any(l => Math.Max(segment.ProjectParameter(l.Segment.Start),
                segment.ProjectParameter(l.Segment.End)) <= _tolerance);
            bool after = group.Any(l => Math.Min(segment.ProjectParameter(l.Segment.Start),
                segment.ProjectParameter(l.Segment.End)) >= segment.Length - _tolerance);
            if(before && after) {
                return group.First();
            }
        }
        return null;
    }

    private static bool HasSource(ContourLine line) {
        return line?.SourceId != null && line.SourceId != ElementId.InvalidElementId;
    }

    private static ContourLine Copy(ContourLine source, Segment2D segment) {
        return new ContourLine {
            Segment = segment,
            Z = source?.Z ?? 0,
            SourceId = source?.SourceId ?? ElementId.InvalidElementId,
            LinkSourceId = source?.LinkSourceId,
            InwardNormal = segment.Direction.PerpendicularLeft()
        };
    }

    private static void AddCuts(Segment2D a, Segment2D b, List<double> cuts) {
        var da = a.Direction;
        var db = b.Direction;
        double cross = da.Cross(db);
        if(Math.Abs(cross) < _angleTolerance) {
            if(Math.Abs((b.Start - a.Start).Cross(da)) < _tolerance) {
                AddCut(a.ProjectParameter(b.Start), a.Length, cuts);
                AddCut(a.ProjectParameter(b.End), a.Length, cuts);
            }
            return;
        }
        double t = (b.Start - a.Start).Cross(db) / cross;
        double u = (b.Start - a.Start).Cross(da) / cross;
        if(u >= -_tolerance && u <= b.Length + _tolerance) {
            AddCut(t, a.Length, cuts);
        }
    }

    private static void AddCut(double value, double length, List<double> cuts) {
        if(value >= -_tolerance && value <= length + _tolerance) {
            cuts.Add(Math.Max(0, Math.Min(length, value)));
        }
    }

    private static List<List<ContourLine>> TraceLoops(List<ContourLine> boundary) {
        var result = new List<List<ContourLine>>();
        while(boundary.Count > 0) {
            var loop = new List<ContourLine> { boundary[0] };
            boundary.RemoveAt(0);
            while(!Same(loop[loop.Count - 1].Segment.End, loop[0].Segment.Start)) {
                var end = loop[loop.Count - 1].Segment.End;
                var next = boundary.Where(l => Same(l.Segment.Start, end)).ToList();
                if(next.Count != 1) {
                    double nearest = boundary.Count == 0 ? double.NaN
                        : boundary.Min(l => l.Segment.Start.DistanceTo(end));
                    double x = UnitUtilsHelper.ConvertFromInternalValue(end.X);
                    double y = UnitUtilsHelper.ConvertFromInternalValue(end.Y);
                    double gap = UnitUtilsHelper.ConvertFromInternalValue(nearest);
                    throw new InvalidOperationException(
                        $"Общая область: незамкнутый или неоднозначный стык контура; "
                        + $"узел ({x:0.#########}; {y:0.#########}) мм; "
                        + $"продолжений {next.Count}; ближайшее начало {gap:0.#########} мм; "
                        + $"осталось отрезков {boundary.Count}");
                }
                loop.Add(next[0]);
                boundary.Remove(next[0]);
            }
            if(loop.Count < 3) {
                throw new InvalidOperationException("Общая область: вырожденная петля");
            }
            result.Add(loop);
        }
        if(result.Count == 0 || result.Count(l => SignedArea(l) > 0) != 1) {
            throw new InvalidOperationException("Общая область: требуется один связный внешний контур");
        }
        return result;
    }

    private static bool Same(Point2D a, Point2D b) => a.DistanceTo(b) < _tolerance;

    private static double SignedArea(List<ContourLine> lines) {
        return lines.Sum(l => l.Segment.Start.Cross(l.Segment.End)) * 0.5;
    }

    private static bool ContainsRoom(RoomContour room, Point2D point) {
        return room.Loops.Where(l => l.IsOuter).Any(l => ContainsPolygon(
                   l.Lines.Select(e => e.Segment.Start).ToList(), point))
               && !room.Loops.Where(l => !l.IsOuter).Any(l => ContainsPolygon(
                   l.Lines.Select(e => e.Segment.Start).ToList(), point));
    }

    private static bool ContainsPolygon(List<Point2D> points, Point2D point) {
        bool inside = false;
        for(int i = 0, j = points.Count - 1; i < points.Count; j = i++) {
            var a = points[i];
            var b = points[j];
            if((a.Y > point.Y) != (b.Y > point.Y)
               && point.X < (b.X - a.X) * (point.Y - a.Y) / (b.Y - a.Y) + a.X) {
                inside = !inside;
            }
        }
        return inside;
    }
}
