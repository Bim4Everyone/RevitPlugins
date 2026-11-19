using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.RoomDimensions.Geometry;
using RevitDocumenter.Models.RoomDimensions.Settings;

namespace RevitDocumenter.Models.RoomDimensions.Contours;

/// <summary>
/// Собирает области через разделители и маячковые стены, не изменяя помещения Revit.
/// </summary>
internal class DimensioningAreaBuilder {
    private const string _beaconParameter = "ФОП_ТИП_Назначение";
    private const double _tolerance = 1e-7;
    private readonly Document _document;
    private readonly RoomContourBuilder _contourBuilder;

    public DimensioningAreaBuilder(Document document, RoomContourBuilder contourBuilder) {
        _document = document.ThrowIfNull();
        _contourBuilder = contourBuilder.ThrowIfNull();
    }

    public List<RoomRunResult> Build(List<RoomRunResult> rooms, AreaSettings settings, List<string> diagnostics) {
        var ignored = Classify(rooms, settings, diagnostics, out var separators);
        // Разделитель может остаться внешней рамкой: запрещаем привязки только к исключаемым стенам.
        var beaconIds = new HashSet<ElementId>(ignored.Except(separators));
        var contours = rooms.Select(r => r.Contour).ToList();
        var adjacency = contours.ToDictionary(c => c, _ => new HashSet<RoomContour>());
        var bridges = new List<(RoomContour A, RoomContour B, List<Point2D> Polygon, ElementId Source)>();
        var coverage = new Dictionary<ContourLine, List<(double Start, double End)>>();
        var bySource = contours.SelectMany(c => c.Loops.SelectMany(l => l.Lines)
                .Where(l => l.SourceId != null && ignored.Contains(l.SourceId))
                .Select(l => (Contour: c, Line: l)))
            .GroupBy(x => x.Line.SourceId);
        foreach(var group in bySource) {
            var entries = group.ToList();
            // Удаляем стену только при наличии границ помещений с обеих сторон.
            if(beaconIds.Contains(group.Key) && entries.All(e => e.Contour == entries[0].Contour)) {
                beaconIds.Remove(group.Key);
                diagnostics.Add($"Маячковая стена {group.Key}: сохраняется как граница без соседнего помещения на виде");
                continue;
            }
            for(int i = 0; i < entries.Count; i++) {
                for(int j = i + 1; j < entries.Count; j++) {
                    var a = entries[i];
                    var b = entries[j];
                    if(a.Contour == b.Contour || a.Contour.Error != null || b.Contour.Error != null
                       || !TryBridge(a.Line, b.Line, out var polygon, coverage)) {
                        continue;
                    }
                    adjacency[a.Contour].Add(b.Contour);
                    adjacency[b.Contour].Add(a.Contour);
                    bridges.Add((a.Contour, b.Contour, polygon, group.Key));
                }
            }
        }

        foreach(var id in separators) {
            bool shared = bridges.Any(b => b.Source == id);
            diagnostics.Add($"Разделитель {id}: " + (shared
                ? "общие внутренние участки исключаются; внешние участки сохраняются"
                : "одиночная внешняя граница сохраняется"));
        }

        var results = new List<RoomRunResult>();
        var visited = new HashSet<RoomContour>();
        foreach(var first in contours) {
            if(!visited.Add(first)) {
                continue;
            }
            var members = new List<RoomContour> { first };
            for(int i = 0; i < members.Count; i++) {
                foreach(var neighbor in adjacency[members[i]]) {
                    if(visited.Add(neighbor)) {
                        members.Add(neighbor);
                    }
                }
            }
            var area = new RoomContour(first.RoomType) { BoundaryLocation = first.BoundaryLocation };
            area.IgnoredSourceIds.UnionWith(beaconIds);
            var result = new RoomRunResult(area, members);
            results.Add(result);
            try {
                var strips = new List<List<Point2D>>();
                var widths = new Dictionary<ElementId, double>();
                var memberLines = members.SelectMany(c => c.Loops).SelectMany(l => l.Lines).ToList();
                var retained = memberLines.Where(l => l.SourceId is null || !beaconIds.Contains(l.SourceId)).ToList();
                foreach(var faces in memberLines.Where(l => l.SourceId != null && beaconIds.Contains(l.SourceId))
                            .GroupBy(l => l.SourceId)) {
                    if(!bridges.Any(b => b.Source == faces.Key && members.Contains(b.A) && members.Contains(b.B))) {
                        throw new InvalidOperationException(
                            $"Игнорируемая стена {faces.Key}: соседнее помещение не найдено на виде");
                    }
                    var polygon = new IgnoredWallStripBuilder().Build(faces.ToList(), retained, out double width);
                    strips.Add(polygon);
                    widths.Add(faces.Key, width);
                    string coordinates = string.Join("; ", polygon.Select(p =>
                        $"{UnitUtilsHelper.ConvertFromInternalValue(p.X):0.#},"
                        + $"{UnitUtilsHelper.ConvertFromInternalValue(p.Y):0.#}"));
                    result.PlanningLog.Add($"Полная полоса стены {faces.Key}, "
                        + $"толщина {UnitUtilsHelper.ConvertFromInternalValue(width):0.#} мм: [{coordinates}]");
                }
                ValidateMembers(members, beaconIds, coverage, widths);
                if(members.Count == 1) {
                    // Нет игнорируемой границы: тот же путь нормализации, что до областей.
                    var copies = first.Loops.Select(l => l.Lines.Select(line => new ContourLine {
                        Segment = line.Segment,
                        Z = line.Z,
                        SourceId = line.SourceId,
                        LinkSourceId = line.LinkSourceId,
                        IsArc = line.IsArc
                    }).ToList()).ToList();
                    _contourBuilder.CompleteArea(area, copies);
                } else {
                    var loops = new AreaBoundaryUnion().Build(members, strips, beaconIds);
                    _contourBuilder.CompleteArea(area, loops);
                    result.PlanningLog.Add($"Область: помещений {members.Count}; полос стен {strips.Count}");
                    foreach(var bridge in bridges.Where(b => members.Contains(b.A) && members.Contains(b.B))) {
                        string coordinates = string.Join("; ", bridge.Polygon.Select(p =>
                            $"{UnitUtilsHelper.ConvertFromInternalValue(p.X):0.#},"
                            + $"{UnitUtilsHelper.ConvertFromInternalValue(p.Y):0.#}"));
                        result.PlanningLog.Add($"Соединение {bridge.A.Room.Number}/{bridge.B.Room.Number}; "
                            + $"источник {bridge.Source}; полоса, мм [{coordinates}]");
                    }
                }
                // Короткие соединители после нормализации не могут вернуть исключённую привязку.
                if(area.Loops.SelectMany(l => l.Lines).Any(l => l.SourceId != null && beaconIds.Contains(l.SourceId))) {
                    throw new InvalidOperationException("На внешней границе области остался игнорируемый элемент");
                }
            } catch(InvalidOperationException ex) {
                area.Error = ex.Message;
                result.PlanningLog.Add(ex.Message);
            }
        }
        return results;
    }

    private HashSet<ElementId> Classify(
        List<RoomRunResult> rooms, AreaSettings settings, List<string> diagnostics,
        out HashSet<ElementId> separators) {
        separators = [];
        var ignored = new HashSet<ElementId>();
        string beaconText = AreaSettings.NormalizeBeaconPartitionText(settings.BeaconPartitionText);
        var ids = rooms.SelectMany(r => r.Contour.Loops).SelectMany(l => l.Lines)
            .Where(l => l.LinkSourceId is null || l.LinkSourceId == ElementId.InvalidElementId)
            .Select(l => l.SourceId).Where(id => id != null && id != ElementId.InvalidElementId).Distinct();
        foreach(var id in ids) {
            var source = _document.GetElement(id);
            if(settings.IgnoreRoomSeparationLines && source is CurveElement
               && source.Category?.Id == new ElementId(BuiltInCategory.OST_RoomSeparationLines)) {
                ignored.Add(id);
                separators.Add(id);
            }
            if(!settings.IgnoreBeaconPartitions || source is not Wall wall) {
                continue;
            }
            // Прямое уточнение пользователя: признак находится только у экземпляра стены.
            var parameters = wall.GetParameters(_beaconParameter);
            if(parameters.Count > 1) {
                throw new InvalidOperationException($"Стена {id}: параметр {_beaconParameter} неоднозначен");
            }
            var parameter = parameters.FirstOrDefault();
            string value = parameter?.StorageType == StorageType.String ? parameter.AsString() : null;
            bool beacon = value != null && value.IndexOf(beaconText, StringComparison.OrdinalIgnoreCase) >= 0;
            if(beacon) {
                ignored.Add(id);
            }
            diagnostics.Add($"Стена {id}; экземпляр; {_beaconParameter}={value ?? "<нет значения>"}; "
                            + (beacon ? "совпало с настройкой маячковой стены" : "сохраняется"));
        }
        return ignored;
    }

    private static void ValidateMembers(
        List<RoomContour> members, HashSet<ElementId> ignored,
        Dictionary<ContourLine, List<(double Start, double End)>> coverage,
        Dictionary<ElementId, double> widths) {
        var first = members[0];
        foreach(var member in members) {
            if(member.Error != null) {
                throw new InvalidOperationException(member.Error);
            }
            if(member.RoomType != first.RoomType || member.Room.LevelId != first.Room.LevelId
               || member.Room.CreatedPhaseId != first.Room.CreatedPhaseId) {
                throw new InvalidOperationException(
                    "Общая область: несовместимые настройки, уровни или фазы помещений");
            }
            double allowance = Math.Max(_tolerance,
                UnitUtilsHelper.ConvertToInternalValue(member.RoomType.MergeOffsetMm));
            foreach(var line in member.Loops.SelectMany(l => l.Lines)
                        .Where(l => l.SourceId != null && ignored.Contains(l.SourceId))) {
                // Короткий разрыв в попарном перекрытии допустим только после восстановления
                // полной полосы конкретной стены. Границу узла ограничивает измеренная толщина.
                double nodeAllowance = allowance + widths[line.SourceId];
                if(!coverage.TryGetValue(line, out var intervals)) {
                    if(line.Segment.Length <= nodeAllowance) {
                        continue;
                    }
                    throw new InvalidOperationException("Игнорируемая стена: нет перекрытия с соседним помещением");
                }
                double end = 0;
                foreach(var interval in intervals.OrderBy(v => v.Start)) {
                    if(interval.Start - end > nodeAllowance) {
                        throw new InvalidOperationException(
                            "Игнорируемая граница: неполное перекрытие с соседними помещениями");
                    }
                    end = Math.Max(end, interval.End);
                }
                if(line.Segment.Length - end > nodeAllowance) {
                    throw new InvalidOperationException(
                            "Игнорируемая граница: неполное перекрытие с соседними помещениями");
                }
            }
        }
    }

    private static bool TryBridge(
        ContourLine a, ContourLine b, out List<Point2D> polygon,
        Dictionary<ContourLine, List<(double Start, double End)>> coverage) {
        polygon = [];
        if(a.IsArc || b.IsArc || Math.Abs(a.Z - b.Z) > _tolerance
           || !a.Segment.IsParallelTo(b.Segment, 1e-6) || a.InwardNormal.Dot(b.InwardNormal) > -0.999) {
            return false;
        }
        double p = a.Segment.ProjectParameter(b.Segment.Start);
        double q = a.Segment.ProjectParameter(b.Segment.End);
        double start = Math.Max(0, Math.Min(p, q));
        double end = Math.Min(a.Segment.Length, Math.Max(p, q));
        if(end - start <= _tolerance) {
            return false;
        }
        var shift = a.Segment.Direction.PerpendicularLeft()
                    * (b.Segment.Start - a.Segment.Start).Dot(a.Segment.Direction.PerpendicularLeft());
        if(shift.Dot(a.InwardNormal) > _tolerance) {
            return false;
        }
        var from = a.Segment.Start + a.Segment.Direction * start;
        var to = a.Segment.Start + a.Segment.Direction * end;
        AddCoverage(a, start, end, coverage);
        double bStart = b.Segment.ProjectParameter(from + shift);
        double bEnd = b.Segment.ProjectParameter(to + shift);
        AddCoverage(b, Math.Min(bStart, bEnd), Math.Max(bStart, bEnd), coverage);
        if(shift.Length > _tolerance) {
            polygon = [from, to, to + shift, from + shift];
        }
        return true;
    }

    private static void AddCoverage(
        ContourLine line, double start, double end,
        Dictionary<ContourLine, List<(double Start, double End)>> coverage) {
        if(!coverage.TryGetValue(line, out var intervals)) {
            intervals = [];
            coverage[line] = intervals;
        }
        intervals.Add((start, end));
    }
}
