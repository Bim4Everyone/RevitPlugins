using System;
using System.Collections.Generic;
using System.Linq;

using RevitDocumenter.Models.RoomDimensions.Contours;
using RevitDocumenter.Models.RoomDimensions.Geometry;
using RevitDocumenter.Models.RoomDimensions.Layout;

namespace RevitDocumenter.Models.RoomDimensions.Planning;

/// <summary>
/// Планирует размеры помещения: по контурам (тип 1) - виртуальный размер на каждую линию;
/// по внутренним контурам (тип 3) - от граней пилонов до ближайших стен или пилонов;
/// затем удаление дублей, сдвижка в линию и объединение в цепочки;
/// в конце общие размеры (тип 2) между крайними плоскостями внешнего контура.
/// Работает с 2D-моделью, размеры в Revit не создает.
/// </summary>
/// <remarks>
/// Отложенные варианты (записаны в схеме):
/// - из дублей оставлять тот, на стороне которого меньше других размеров (сейчас - верхний/левый);
/// - объединять в цепочки только размеры одной петли (сейчас - разрешено между петлями помещения).
/// </remarks>
internal class ContourDimensionPlanner {
    private const double _angleTolerance = 1e-4;
    private const double _axisTolerance = 1e-6;

    public List<VirtualDimension> Plan(RoomContour contour, PlanningOptions options, List<string> log) {
        contour.ThrowIfNull();
        options.ThrowIfNull();
        log.ThrowIfNull();

        var dimensions = CreateVirtualDimensions(contour, options, log);

        // У пилона оставляем по одному собственному размеру на ось: от него строится тип 3
        dimensions = RemoveDuplicates(dimensions, options, log, d => d.Kind == "Contour" && d.InvolvesInner);

        // Тип 3 строится от оставшихся размеров пилонов, чтобы встать на их линию.
        // Встречные размеры между двумя пилонами убираются сразу.
        dimensions.AddRange(CreateInternalDimensions(contour, dimensions, options, log));
        dimensions = RemoveDuplicates(dimensions, options, log, d => d.Kind == "Internal");

        dimensions = MergeIntoChains(dimensions, options, log);

        // Дубли удаляются после объединения: убирается только одиночный размер,
        // повторяющий другой одиночный размер или сегмент цепочки
        dimensions = RemoveDuplicateSingles(dimensions, options, log);

        dimensions.AddRange(CreateOverallDimensions(contour, dimensions, options, log));

        for(int i = 0; i < dimensions.Count; i++) {
            dimensions[i].Id = i;
        }
        return dimensions;
    }

    /// <summary>
    /// Виртуальный размер на каждую линию: между плоскостями предыдущего и следующего элемента контура.
    /// </summary>
    private static List<VirtualDimension> CreateVirtualDimensions(
        RoomContour contour,
        PlanningOptions options,
        List<string> log) {
        var dimensions = new List<VirtualDimension>();
        foreach(var loop in contour.Loops) {
            for(int position = 0; position < loop.Elements.Count; position++) {
                if(loop.Elements[position] is not ContourLine line || line.IsArc || line.IsFiltered) {
                    continue;
                }
                string name = $"{loop.Index}:{line.Index}";
                var previous = loop.GetPrevious(position);
                var next = loop.GetNext(position);

                if(IsParallelLine(previous, line) || IsParallelLine(next, line)) {
                    log.Add($"Линия {name}: соседняя линия параллельна, размер не создан");
                    continue;
                }
                if(previous.Reference is null || next.Reference is null) {
                    log.Add($"Линия {name}: у соседнего элемента нет привязки, размер не создан");
                    continue;
                }

                var direction = Canonical(line.Segment.Direction);
                var perpendicular = direction.PerpendicularLeft();
                bool textFacesContour = DimensionTextHelper.TextFacesContour(direction, line.InwardNormal);
                double lineOffset = options.Offset + (textFacesContour ? options.TextAllowance : 0);

                // Плоскость соседнего элемента проходит через общий конец с линией
                var anchors = new List<DimensionAnchor> {
                    new(previous.Reference, line.Segment.Start.Dot(direction), loop.Index, previous.Index),
                    new(next.Reference, line.Segment.End.Dot(direction), loop.Index, next.Index)
                };

                dimensions.Add(new VirtualDimension {
                    Direction = direction,
                    Side = line.InwardNormal,
                    LinePosition = (line.Segment.Start + line.InwardNormal * lineOffset).Dot(perpendicular),
                    ContourPosition = line.Segment.Start.Dot(perpendicular),
                    Z = line.Z,
                    Anchors = anchors.OrderBy(a => a.Coordinate).ToList(),
                    Sources = [name],
                    Kind = "Contour",
                    SourceLoopIndex = loop.Index,
                    InvolvesInner = !loop.IsOuter
                });
            }
        }
        log.Add($"Создано виртуальных размеров: {dimensions.Count}");
        return dimensions;
    }

    /// <summary>
    /// Тип 3: от граней внутреннего контура (пилона) лучами до ближайшей грани, обращенной к пилону.
    /// Размер ставится на линию собственного размера пилона, чтобы планировщик сшил их в цепочку
    /// стена | пилон | стена. Если на стороне нет подходящей грани, сторона пропускается.
    /// </summary>
    private static List<VirtualDimension> CreateInternalDimensions(
        RoomContour contour,
        List<VirtualDimension> dimensions,
        PlanningOptions options,
        List<string> log) {
        var result = new List<VirtualDimension>();
        var innerLoops = contour.Inners.ToDictionary(l => l.Index);

        foreach(var pylonDimension in dimensions.Where(
                    d => d.Kind == "Contour" && innerLoops.ContainsKey(d.SourceLoopIndex))) {
            var loop = innerLoops[pylonDimension.SourceLoopIndex];
            var perpendicular = pylonDimension.Perpendicular;
            var band = loop.Lines
                .SelectMany(l => new[] { l.Segment.Start.Dot(perpendicular), l.Segment.End.Dot(perpendicular) })
                .ToList();
            double bandMin = band.Min();
            double bandMax = band.Max();

            foreach(int ray in new[] { -1, 1 }) {
                var pylonAnchor = ray < 0 ? pylonDimension.Anchors.First() : pylonDimension.Anchors.Last();
                var target = FindRayTarget(contour, loop, pylonDimension, pylonAnchor.Coordinate, ray, bandMin, bandMax, options);
                string side = ray < 0 ? "назад" : "вперед";
                string name = $"{Names(pylonDimension)}->{side}";
                if(target is null) {
                    log.Add($"Пилон {Names(pylonDimension)}: в направлении {side} нет грани, размер до стены не создан");
                    continue;
                }

                var (line, loopIndex) = target.Value;
                var targetAnchor = new DimensionAnchor(
                    line.Reference,
                    line.Segment.Start.Dot(pylonDimension.Direction),
                    loopIndex,
                    line.Index);
                result.Add(new VirtualDimension {
                    Kind = "Internal",
                    InvolvesInner = true,
                    SourceLoopIndex = pylonDimension.SourceLoopIndex,
                    Direction = pylonDimension.Direction,
                    Side = pylonDimension.Side,
                    LinePosition = pylonDimension.LinePosition,
                    ContourPosition = pylonDimension.ContourPosition,
                    Z = pylonDimension.Z,
                    Anchors = new[] { targetAnchor, pylonAnchor }.OrderBy(a => a.Coordinate).ToList(),
                    Sources = [name]
                });
                log.Add($"Пилон {Names(pylonDimension)}: размер {side} до линии {loopIndex}:{line.Index} "
                        + $"({Mm(Math.Abs(targetAnchor.Coordinate - pylonAnchor.Coordinate))} мм)");
            }
        }
        return result;
    }

    /// <summary>
    /// Ближайшая по лучу линия контура: перпендикулярна размеру, имеет привязку, обращена к пилону
    /// и перекрывает полосу пилона или пересекается размерной линией.
    /// </summary>
    private static (ContourLine Line, int LoopIndex)? FindRayTarget(
        RoomContour contour,
        ContourLoop pylonLoop,
        VirtualDimension pylonDimension,
        double fromCoordinate,
        int ray,
        double bandMin,
        double bandMax,
        PlanningOptions options) {
        var direction = pylonDimension.Direction;
        var perpendicular = pylonDimension.Perpendicular;
        (ContourLine Line, int LoopIndex)? best = null;
        double bestDistance = double.MaxValue;

        foreach(var loop in contour.Loops.Where(l => l != pylonLoop)) {
            foreach(var line in loop.Lines) {
                if(line.IsArc || line.Reference is null
                   || Math.Abs(line.Segment.Direction.Dot(direction)) > _angleTolerance) {
                    continue;
                }
                // Грань должна смотреть на пилон: нормаль в сторону помещения направлена против луча
                if(line.InwardNormal.Dot(direction) * ray >= 0) {
                    continue;
                }
                double distance = (line.Segment.Start.Dot(direction) - fromCoordinate) * ray;
                if(distance <= options.CoordinateTolerance || distance >= bestDistance) {
                    continue;
                }
                double a = line.Segment.Start.Dot(perpendicular);
                double b = line.Segment.End.Dot(perpendicular);
                double lineMin = Math.Min(a, b);
                double lineMax = Math.Max(a, b);
                bool overlapsBand = Math.Min(lineMax, bandMax) - Math.Max(lineMin, bandMin) > options.CoordinateTolerance;
                bool crossesDimensionLine = pylonDimension.LinePosition >= lineMin - options.CoordinateTolerance
                                            && pylonDimension.LinePosition <= lineMax + options.CoordinateTolerance;
                if(!overlapsBand && !crossesDimensionLine) {
                    continue;
                }
                best = (line, loop.Index);
                bestDistance = distance;
            }
        }
        return best;
    }

    /// <summary>
    /// Тип 2: общий размер между крайними плоскостями внешнего контура по каждому направлению размеров.
    /// Ставится, если в самой длинной цепочке направления больше OverallMinSegments сегментов
    /// и нет одиночного размера на тех же плоскостях. Встает следующим ярусом за самой длинной цепочкой.
    /// </summary>
    private static List<VirtualDimension> CreateOverallDimensions(
        RoomContour contour,
        List<VirtualDimension> dimensions,
        PlanningOptions options,
        List<string> log) {
        var result = new List<VirtualDimension>();
        var outer = contour.Outer;
        if(outer is null) {
            return result;
        }

        var directions = new List<Point2D>();
        foreach(var dimension in dimensions) {
            if(!directions.Any(d => IsSameDirection(d, dimension.Direction))) {
                directions.Add(dimension.Direction);
            }
        }

        foreach(var direction in directions) {
            string axis = $"{Math.Round(Math.Atan2(direction.Y, direction.X) * 180 / Math.PI)}°";
            var sameDirection = dimensions.Where(d => IsSameDirection(d.Direction, direction)).ToList();
            int maxSegments = sameDirection.Max(d => d.Anchors.Count - 1);
            if(maxSegments <= options.OverallMinSegments) {
                log.Add($"Общий размер {axis}: в самой длинной цепочке {maxSegments} сегм. "
                        + $"(нужно больше {options.OverallMinSegments}), не нужен");
                continue;
            }

            var extremes = GetExtremeAnchors(outer, direction, options);
            if(extremes is null) {
                log.Add($"Общий размер {axis}: не найдены крайние плоскости с привязкой");
                continue;
            }
            var (minAnchor, maxAnchor) = extremes.Value;

            var existing = sameDirection.FirstOrDefault(
                d => d.Anchors.Count == 2
                     && Math.Abs(d.Min - minAnchor.Coordinate) <= options.CoordinateTolerance
                     && Math.Abs(d.Max - maxAnchor.Coordinate) <= options.CoordinateTolerance);
            if(existing != null) {
                log.Add($"Общий размер {axis} ({Mm(maxAnchor.Coordinate - minAnchor.Coordinate)} мм): "
                        + $"на тех же плоскостях уже есть размер {Names(existing)}, не нужен");
                continue;
            }

            // Опора - цепочка внешнего контура; цепочки с пилонами стоят внутри помещения
            var outerOnly = sameDirection.Where(d => !d.InvolvesInner).ToList();
            var basis = (outerOnly.Count > 0 ? outerOnly : sameDirection)
                .OrderByDescending(d => d.Anchors.Count)
                .ThenByDescending(d => d.LinePosition * d.SideSign)
                .First();
            result.Add(new VirtualDimension {
                Kind = "Overall",
                SourceLoopIndex = outer.Index,
                Direction = direction,
                Side = basis.Side,
                LinePosition = basis.LinePosition + basis.SideSign * options.TierStep,
                ContourPosition = basis.ContourPosition,
                Z = basis.Z,
                Anchors = [minAnchor, maxAnchor],
                Sources = [$"overall {axis}"]
            });
            log.Add($"Общий размер {axis} ({Mm(maxAnchor.Coordinate - minAnchor.Coordinate)} мм) "
                    + $"следующим ярусом за цепочкой {Names(basis)} ({maxSegments} сегм.)");
        }
        return result;
    }

    /// <summary>
    /// Крайние плоскости внешнего контура вдоль направления: линии, перпендикулярные направлению,
    /// и контурные точки. На одной координате берется самая длинная линия.
    /// </summary>
    private static (DimensionAnchor Min, DimensionAnchor Max)? GetExtremeAnchors(
        ContourLoop outer,
        Point2D direction,
        PlanningOptions options) {
        var candidates = new List<(DimensionAnchor Anchor, double Length)>();
        foreach(var element in outer.Elements) {
            if(element.Reference is null) {
                continue;
            }
            switch(element) {
                case ContourLine line when !line.IsArc
                                           && Math.Abs(line.Segment.Direction.Dot(direction)) <= _angleTolerance:
                    candidates.Add((new DimensionAnchor(
                        line.Reference, line.Segment.Start.Dot(direction), outer.Index, line.Index), line.Segment.Length));
                    break;
                case ContourPoint point:
                    candidates.Add((new DimensionAnchor(
                        point.Reference, point.Location.Dot(direction), outer.Index, point.Index), 0));
                    break;
            }
        }
        if(candidates.Count < 2) {
            return null;
        }

        double min = candidates.Min(c => c.Anchor.Coordinate);
        double max = candidates.Max(c => c.Anchor.Coordinate);
        if(max - min <= options.CoordinateTolerance) {
            return null;
        }
        var minAnchor = candidates
            .Where(c => c.Anchor.Coordinate <= min + options.CoordinateTolerance)
            .OrderByDescending(c => c.Length)
            .First().Anchor;
        var maxAnchor = candidates
            .Where(c => c.Anchor.Coordinate >= max - options.CoordinateTolerance)
            .OrderByDescending(c => c.Length)
            .First().Anchor;
        return (minAnchor, maxAnchor);
    }

    /// <summary>
    /// Дубли среди размеров из scope: одно направление, те же или компланарные плоскости привязки.
    /// Остается размер с наибольшим положением по оси Perpendicular: у горизонтальных верхний, у вертикальных левый.
    /// Размеры вне scope не меняются.
    /// </summary>
    private static List<VirtualDimension> RemoveDuplicates(
        List<VirtualDimension> dimensions,
        PlanningOptions options,
        List<string> log,
        Func<VirtualDimension, bool> scope) {
        var ordered = dimensions.Where(scope).OrderByDescending(d => d.LinePosition).ToList();
        var kept = dimensions.Where(d => !scope(d)).ToList();
        int outsideCount = kept.Count;
        foreach(var dimension in ordered) {
            var duplicateOf = kept.Skip(outsideCount).FirstOrDefault(k => IsSameDirection(k, dimension)
                                                       && Math.Abs(k.Min - dimension.Min) <= options.CoordinateTolerance
                                                       && Math.Abs(k.Max - dimension.Max) <= options.CoordinateTolerance);
            if(duplicateOf != null) {
                log.Add($"Размер {Names(dimension)} ({Mm(dimension.Max - dimension.Min)} мм) - дубль {Names(duplicateOf)}, удален");
                continue;
            }
            kept.Add(dimension);
        }
        return kept;
    }

    /// <summary>
    /// Удаляет одиночный размер, если такой же уже есть сегментом цепочки или другим одиночным размером.
    /// Цепочки не удаляются. Из одиночных дублей остается верхний/левый.
    /// </summary>
    private static List<VirtualDimension> RemoveDuplicateSingles(
        List<VirtualDimension> dimensions,
        PlanningOptions options,
        List<string> log) {
        var kept = dimensions.Where(d => d.Anchors.Count > 2).ToList();
        foreach(var single in dimensions.Where(d => d.Anchors.Count == 2).OrderByDescending(d => d.LinePosition)) {
            var duplicateOf = kept.FirstOrDefault(k => IsSameDirection(k, single) && ContainsSegment(k, single, options));
            if(duplicateOf != null) {
                log.Add($"Размер {Names(single)} ({Mm(single.Max - single.Min)} мм) повторяет "
                        + $"{(duplicateOf.Anchors.Count > 2 ? "сегмент цепочки" : "размер")} {Names(duplicateOf)}, удален");
                continue;
            }
            kept.Add(single);
        }
        return kept;
    }

    /// <summary>
    /// Есть ли у размера сегмент между теми же плоскостями, что и у одиночного размера.
    /// </summary>
    private static bool ContainsSegment(VirtualDimension dimension, VirtualDimension single, PlanningOptions options) {
        return dimension.Anchors
            .Zip(dimension.Anchors.Skip(1), (a, b) => (a.Coordinate, b.Coordinate))
            .Any(s => Math.Abs(s.Item1 - single.Min) <= options.CoordinateTolerance
                      && Math.Abs(s.Item2 - single.Max) <= options.CoordinateTolerance);
    }

    /// <summary>
    /// Объединяет размеры с общей плоскостью в цепочки. Цепочка ставится на линию самого дальнего
    /// от контура размера группы. Размер, которому для этого нужен сдвиг больше MaxShift
    /// или выноска длиннее MaxLeader, остается отдельным.
    /// </summary>
    private static List<VirtualDimension> MergeIntoChains(
        List<VirtualDimension> dimensions,
        PlanningOptions options,
        List<string> log) {
        var result = new List<VirtualDimension>();
        foreach(var component in GetConnectedComponents(dimensions, options)) {
            ProcessComponent(component, options, log, result);
        }
        return result;
    }

    private static void ProcessComponent(
        List<VirtualDimension> component,
        PlanningOptions options,
        List<string> log,
        List<VirtualDimension> result) {
        if(component.Count == 1) {
            result.Add(component[0]);
            return;
        }

        // Самый дальний от контура размер задает линию цепочки
        var farthest = component.OrderByDescending(d => d.LinePosition * d.SideSign).First();
        double target = farthest.LinePosition;
        bool fits = component.All(d => Math.Abs(target - d.LinePosition) <= options.MaxShift
                                       && Math.Abs(target - d.ContourPosition) <= options.MaxLeader);
        if(fits) {
            result.Add(CreateChain(component, target, options));
            log.Add($"Цепочка из {Names(component)} на линии размера {Names(farthest)}");
            return;
        }

        // Дальний размер не дает остальным встать в линию: оставляем его отдельно и пробуем снова
        log.Add($"Размер {Names(farthest)} не вошел в цепочку: превышен сдвиг или длина выноски");
        result.Add(farthest);
        var rest = component.Where(d => d != farthest).ToList();
        foreach(var subComponent in GetConnectedComponents(rest, options)) {
            ProcessComponent(subComponent, options, log, result);
        }
    }

    private static VirtualDimension CreateChain(
        List<VirtualDimension> members,
        double linePosition,
        PlanningOptions options) {
        var anchors = new List<DimensionAnchor>();
        foreach(var anchor in members.SelectMany(m => m.Anchors).OrderBy(a => a.Coordinate)) {
            // Совпадающие плоскости соседних размеров берем один раз
            if(anchors.Count > 0
               && Math.Abs(anchors[anchors.Count - 1].Coordinate - anchor.Coordinate) <= options.CoordinateTolerance) {
                continue;
            }
            anchors.Add(anchor);
        }

        var first = members[0];
        return new VirtualDimension {
            Kind = "Chain",
            SourceLoopIndex = first.SourceLoopIndex,
            InvolvesInner = members.Any(m => m.InvolvesInner),
            Direction = first.Direction,
            Side = first.Side,
            LinePosition = linePosition,
            ContourPosition = members.OrderByDescending(m => m.ContourPosition * m.SideSign).First().ContourPosition,
            Z = first.Z,
            Anchors = anchors,
            Sources = members.SelectMany(m => m.Sources).ToList()
        };
    }

    /// <summary>
    /// Компоненты связности: размеры связаны, если у них одно направление и сторона, общая плоскость
    /// привязки, они не перекрываются и их линии отстоят не больше чем на MaxShift.
    /// </summary>
    private static List<List<VirtualDimension>> GetConnectedComponents(
        List<VirtualDimension> dimensions,
        PlanningOptions options) {
        int count = dimensions.Count;
        int[] parent = Enumerable.Range(0, count).ToArray();

        int Find(int x) {
            while(parent[x] != x) {
                parent[x] = parent[parent[x]];
                x = parent[x];
            }
            return x;
        }

        for(int i = 0; i < count; i++) {
            for(int j = i + 1; j < count; j++) {
                if(CanChain(dimensions[i], dimensions[j], options)) {
                    parent[Find(i)] = Find(j);
                }
            }
        }

        return Enumerable.Range(0, count)
            .GroupBy(Find)
            .Select(g => g.Select(i => dimensions[i]).ToList())
            .ToList();
    }

    private static bool CanChain(VirtualDimension a, VirtualDimension b, PlanningOptions options) {
        if(!IsSameDirection(a, b) || a.SideSign != b.SideSign) {
            return false;
        }
        if(Math.Abs(a.LinePosition - b.LinePosition) > options.MaxShift) {
            return false;
        }
        double overlap = Math.Min(a.Max, b.Max) - Math.Max(a.Min, b.Min);
        if(overlap > options.CoordinateTolerance) {
            return false;
        }
        return a.Anchors.Any(x => b.Anchors.Any(y => Math.Abs(x.Coordinate - y.Coordinate) <= options.CoordinateTolerance));
    }

    private static bool IsSameDirection(VirtualDimension a, VirtualDimension b) {
        return IsSameDirection(a.Direction, b.Direction);
    }

    private static bool IsSameDirection(Point2D a, Point2D b) {
        return Math.Abs(a.Cross(b)) < _angleTolerance && a.Dot(b) > 0;
    }

    private static bool IsParallelLine(ContourElement neighbor, ContourLine line) {
        return neighbor is ContourLine neighborLine
               && neighborLine.Segment.IsParallelTo(line.Segment, _angleTolerance);
    }

    /// <summary>
    /// Направление размера как его читает Revit: слева направо, вертикальные снизу вверх.
    /// </summary>
    private static Point2D Canonical(Point2D direction) {
        var normalized = direction.Normalize();
        return normalized.X < -_axisTolerance || (Math.Abs(normalized.X) <= _axisTolerance && normalized.Y < 0)
            ? -normalized
            : normalized;
    }

    private static string Names(VirtualDimension dimension) {
        return "[" + string.Join(", ", dimension.Sources) + "]";
    }

    private static string Names(IEnumerable<VirtualDimension> dimensions) {
        return "[" + string.Join(", ", dimensions.SelectMany(d => d.Sources)) + "]";
    }

    private static string Mm(double feet) {
        return UnitUtilsHelper.ConvertFromInternalValue(feet).ToString("0.#");
    }
}
