using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.RoomDimensions.Geometry;

namespace RevitDocumenter.Models.RoomDimensions.Contours;

/// <summary>
/// Нормализует петлю контура: убирает мелкие ступеньки, сращивает попутные линии
/// и помечает короткие линии, которые не образмериваются.
/// Работает только с 2D-геометрией, Revit API не вызывает.
/// </summary>
internal class ContourNormalizer {
    // Допуск параллельности по синусу угла
    private const double _angleTolerance = 1e-4;
    // Линии с разницей отступа меньше этого значения считаются лежащими на одной прямой, футы (~0.3 мм)
    private const double _sameLineTolerance = 1e-3;
    // Численная соосность граней по всем концам, мм модели; не заменяет MergeOffset.
    private const double _numericalOffsetMm = 0.1;
    private const double _numericalDirectionDot = 0.999;
    private const double _maxNumericalReturnMm = 1.0;

    /// <param name="lines">Линии петли по порядку обхода. Нормали внутрь уже должны быть заданы.</param>
    /// <param name="mergeOffset">Отступ между параллельными линиями, меньше которого они сращиваются, футы.</param>
    /// <param name="minLength">Линии короче этого значения не образмериваются, футы.</param>
    /// <param name="log">Журнал действий нормализации.</param>
    /// <returns>Нормализованные линии петли.</returns>
    public List<ContourLine> Normalize(
        List<ContourLine> lines,
        double mergeOffset,
        double minLength,
        List<string> log) {
        lines.ThrowIfNull();
        log.ThrowIfNull();

        var result = lines.ToList();
        bool changed;
        do {
            changed = result.Count > 3 && (RemoveNumericalReturn(result, mergeOffset, log)
                || RemoveNumericalConnector(result, mergeOffset, log)
                || RemoveStep(result, mergeOffset, log) || MergeAdjacent(result, mergeOffset, log));
        } while(changed);

        MarkShortLines(result, minLength, log);
        return result;
    }

    /// <summary>
    /// Убирает короткий обратный ход и соосную связку без источников у прямого угла.
    /// Две реальные грани доводятся до пересечения их опорных прямых.
    /// </summary>
    private static bool RemoveNumericalReturn(List<ContourLine> lines, double mergeOffset, List<string> log) {
        if(lines.Count < 5 || mergeOffset <= 0) {
            return false;
        }

        double tolerance = Math.Min(mergeOffset, UnitUtilsHelper.ConvertToInternalValue(_numericalOffsetMm));
        double maxReturn = UnitUtilsHelper.ConvertToInternalValue(_maxNumericalReturnMm + 1e-6);
        int count = lines.Count;
        for(int i = 0; i < count; i++) {
            var first = lines[i];
            var second = lines[(i + 1) % count];
            var before = lines[(i + count - 1) % count];
            var after = lines[(i + 2) % count];
            if(first.IsArc || second.IsArc || before.IsArc || after.IsArc
               || HasSource(first) || HasSource(second) || !HasSource(before) || !HasSource(after)
               || first.Segment.Direction.Dot(second.Segment.Direction) > -_numericalDirectionDot) {
                continue;
            }

            bool returnFirst = first.Segment.Length < second.Segment.Length;
            var backtrack = returnFirst ? first : second;
            var connector = returnFirst ? second : first;
            var basis = returnFirst ? after : before;
            var transverse = returnFirst ? before : after;
            if(backtrack.Segment.Length > maxReturn
               || connector.Segment.Direction.Dot(basis.Segment.Direction) < _numericalDirectionDot
               || !transverse.Segment.IsPerpendicularTo(basis.Segment, _angleTolerance)) {
                continue;
            }

            var normal = basis.Segment.Direction.PerpendicularLeft();
            var points = new[] { first.Segment.Start, first.Segment.End, second.Segment.End };
            double deviation = points.Max(p => Math.Abs((p - basis.Segment.Start).Dot(normal)));
            var transverseEnd = returnFirst ? before.Segment.End : after.Segment.Start;
            if(deviation > tolerance || !TryIntersect(before.Segment, after.Segment, out var corner)
               || transverseEnd.DistanceTo(corner) > tolerance
               || (corner - before.Segment.Start).Dot(before.Segment.Direction) <= 0
               || (after.Segment.End - corner).Dot(after.Segment.Direction) <= 0) {
                continue;
            }

            before.Segment = new Segment2D(before.Segment.Start, corner);
            after.Segment = new Segment2D(corner, after.Segment.End);
            lines.Remove(first);
            lines.Remove(second);
            log.Add($"Численный обратный ход {Raw(backtrack)} ({Mm(backtrack.Segment.Length)} мм) "
                    + $"и связка {Raw(connector)} убраны между {Raw(before)} и {Raw(after)}");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Убирает ложный наклон связки без источника между реальными соосными гранями.
    /// Сразу сращивает соседние грани, чтобы в петле не осталось разрыва.
    /// </summary>
    private static bool RemoveNumericalConnector(List<ContourLine> lines, double mergeOffset, List<string> log) {
        // Удаление связки и сращивание её соседей сокращают петлю на две линии.
        if(lines.Count < 5 || mergeOffset <= 0) {
            return false;
        }
        double tolerance = UnitUtilsHelper.ConvertToInternalValue(_numericalOffsetMm);
        for(int i = 0; i < lines.Count; i++) {
            var line = lines[i];
            var previous = lines[(i - 1 + lines.Count) % lines.Count];
            var next = lines[(i + 1) % lines.Count];
            if(HasSource(line) || line.IsArc || !HasSource(previous) || !HasSource(next)
               || !AreCodirectional(previous, next)
               || line.Segment.Direction.Dot(previous.Segment.Direction) < _numericalDirectionDot
               || GetOffset(previous, next) >= mergeOffset) {
                continue;
            }

            var origin = previous.Segment.Start;
            var normal = previous.Segment.Direction.PerpendicularLeft();
            var points = new[] { line.Segment.Start, line.Segment.End, next.Segment.Start, next.Segment.End };
            double deviation = points.Max(p => Math.Abs((p - origin).Dot(normal)));
            if(deviation > tolerance) {
                continue;
            }

            var (merged, _) = Merge(previous, next);
            lines.RemoveAt(i);
            int previousIndex = (i - 1 + lines.Count) % lines.Count;
            ReplaceAdjacent(lines, previousIndex, merged);
            log.Add($"Численная связка {Raw(line)} ({Mm(line.Segment.Length)} мм) без источника удалена "
                    + $"между {Raw(previous)} и {Raw(next)}: отклонение "
                    + $"{UnitUtilsHelper.ConvertFromInternalValue(deviation):0.######} мм");
            return true;
        }
        return false;
    }

    private static bool HasSource(ContourLine line) {
        return line.SourceId != null && line.SourceId != ElementId.InvalidElementId;
    }

    /// <summary>
    /// Убирает короткую линию между двумя попутными параллельными линиями с малым отступом.
    /// После этого соседние линии срастаются на следующем проходе.
    /// </summary>
    private static bool RemoveStep(List<ContourLine> lines, double mergeOffset, List<string> log) {
        int count = lines.Count;
        for(int i = 0; i < count; i++) {
            var line = lines[i];
            var previous = lines[(i - 1 + count) % count];
            var next = lines[(i + 1) % count];
            if(line.Segment.Length >= mergeOffset
               || !AreCodirectional(previous, next)
               || GetOffset(previous, next) >= mergeOffset
               || line.Segment.IsParallelTo(previous.Segment, _angleTolerance)) {
                continue;
            }

            lines.RemoveAt(i);
            log.Add($"Ступенька {Raw(line)} ({Mm(line.Segment.Length)} мм) удалена "
                    + $"между линиями {Raw(previous)} и {Raw(next)}");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Сращивает две соседние попутные линии, если отступ между ними меньше допуска.
    /// </summary>
    private static bool MergeAdjacent(List<ContourLine> lines, double mergeOffset, List<string> log) {
        int count = lines.Count;
        for(int i = 0; i < count; i++) {
            int j = (i + 1) % count;
            var first = lines[i];
            var second = lines[j];
            if(!AreCodirectional(first, second) || GetOffset(first, second) >= mergeOffset) {
                continue;
            }

            var (merged, basis) = Merge(first, second);
            log.Add($"Линии {Raw(first)} и {Raw(second)} срощены в {Raw(merged)} "
                    + $"({Mm(merged.Segment.Length)} мм, отступ {Mm(GetOffset(first, second))} мм, "
                    + $"прямая и источник от линии {Raw(basis)})");

            ReplaceAdjacent(lines, i, merged);
            return true;
        }
        return false;
    }

    private static void ReplaceAdjacent(List<ContourLine> lines, int index, ContourLine merged) {
        int next = (index + 1) % lines.Count;
        lines[index] = merged;
        lines.RemoveAt(next);
        ConnectNeighbors(lines, next == 0 ? index - 1 : index);
    }

    /// <summary>
    /// Помечает короткие линии. Они остаются в контуре как привязка для соседних размеров,
    /// но сами не образмериваются.
    /// </summary>
    private static void MarkShortLines(List<ContourLine> lines, double minLength, List<string> log) {
        foreach(var line in lines.Where(l => l.Segment.Length < minLength)) {
            line.IsFiltered = true;
            log.Add($"Линия {Raw(line)} ({Mm(line.Segment.Length)} мм) короче минимальной длины, не образмеривается");
        }
    }

    /// <summary>
    /// Сращивает две попутные линии. Новая линия лежит на прямой, на которую приходится
    /// наибольшая суммарная длина исходных линий группы. Источник, отметка и нормаль
    /// берутся от самой длинной исходной линии на этой прямой.
    /// </summary>
    private static (ContourLine Merged, ContourLine Basis) Merge(ContourLine first, ContourLine second) {
        var parts = GetParts(first).Concat(GetParts(second)).ToList();
        var basis = ChooseBasis(parts);
        var origin = basis.Segment.Start;
        var direction = basis.Segment.Direction;

        Point2D Project(Point2D point) => origin + direction * (point - origin).Dot(direction);

        var merged = new ContourLine {
            Segment = new Segment2D(Project(first.Segment.Start), Project(second.Segment.End)),
            Z = basis.Z,
            InwardNormal = basis.InwardNormal,
            SourceId = basis.SourceId,
            LinkSourceId = basis.LinkSourceId,
            IsArc = false,
            RawIndices = first.RawIndices.Concat(second.RawIndices).ToList(),
            MergedParts = parts
        };
        return (merged, basis);
    }

    private static List<ContourLine> GetParts(ContourLine line) {
        return line.MergedParts ?? new List<ContourLine> { line };
    }

    /// <summary>
    /// Группирует исходные линии по прямым и выбирает самую длинную линию на прямой
    /// с наибольшей суммарной длиной.
    /// </summary>
    private static ContourLine ChooseBasis(List<ContourLine> parts) {
        var normal = parts[0].Segment.Direction.PerpendicularLeft();
        var origin = parts[0].Segment.Start;
        var groups = new List<List<ContourLine>>();
        foreach(var part in parts) {
            double offset = (part.Segment.Start - origin).Dot(normal);
            var group = groups.FirstOrDefault(
                g => Math.Abs((g[0].Segment.Start - origin).Dot(normal) - offset) < _sameLineTolerance);
            if(group is null) {
                groups.Add([part]);
            } else {
                group.Add(part);
            }
        }
        return groups
            .OrderByDescending(g => g.Sum(p => p.Segment.Length))
            .First()
            .OrderByDescending(p => p.Segment.Length)
            .First();
    }

    /// <summary>
    /// Подрезает или продлевает соседние линии до сращённой, чтобы контур оставался замкнутым.
    /// </summary>
    private static void ConnectNeighbors(List<ContourLine> lines, int index) {
        int count = lines.Count;
        var merged = lines[index];
        var previous = lines[(index - 1 + count) % count];
        var next = lines[(index + 1) % count];

        if(previous != merged && TryIntersect(previous.Segment, merged.Segment, out var start)) {
            previous.Segment = new Segment2D(previous.Segment.Start, start);
            merged.Segment = new Segment2D(start, merged.Segment.End);
        }
        if(next != merged && TryIntersect(next.Segment, merged.Segment, out var end)) {
            next.Segment = new Segment2D(end, next.Segment.End);
            merged.Segment = new Segment2D(merged.Segment.Start, end);
        }
    }

    /// <summary>
    /// Точка пересечения прямых двух отрезков. false для параллельных.
    /// </summary>
    private static bool TryIntersect(Segment2D a, Segment2D b, out Point2D point) {
        var d1 = a.Direction;
        var d2 = b.Direction;
        double cross = d1.Cross(d2);
        if(Math.Abs(cross) < _angleTolerance) {
            point = default;
            return false;
        }
        double t = (b.Start - a.Start).Cross(d2) / cross;
        point = a.Start + d1 * t;
        return true;
    }

    /// <summary>
    /// Линии параллельны, идут в одну сторону и обе прямые.
    /// </summary>
    private static bool AreCodirectional(ContourLine a, ContourLine b) {
        return !a.IsArc && !b.IsArc
               && a.Segment.IsParallelTo(b.Segment, _angleTolerance)
               && a.Segment.Direction.Dot(b.Segment.Direction) > 0;
    }

    /// <summary>
    /// Расстояние между прямыми двух параллельных линий.
    /// </summary>
    private static double GetOffset(ContourLine a, ContourLine b) {
        var normal = a.Segment.Direction.PerpendicularLeft();
        return Math.Abs((b.Segment.Midpoint - a.Segment.Start).Dot(normal));
    }

    private static string Raw(ContourLine line) {
        return "[" + string.Join(",", line.RawIndices) + "]";
    }

    private static string Mm(double feet) {
        return UnitUtilsHelper.ConvertFromInternalValue(feet).ToString("0.#");
    }
}
