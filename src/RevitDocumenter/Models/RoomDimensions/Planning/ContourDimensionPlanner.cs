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
/// - из дублей оставлять тот, на стороне которого меньше других размеров
///   (сейчас - тот, у кого текст не обращен к стене, затем верхний/левый);
/// - удалять дубль, только если между размерными линиями нет других параллельных размеров
///   (сейчас - по расстоянию DuplicateMaxDistance);
/// - объединять в цепочки только размеры одной петли (сейчас - разрешено между петлями помещения).
/// </remarks>
internal class ContourDimensionPlanner {
    private const double _angleTolerance = 1e-4;
    private const double _axisTolerance = 1e-6;
    // Укорочение размерной линии с концов при проверке "внутри помещения", футы (~5 мм)
    private const double _insideTrim = 5.0 / 304.8;

    public List<VirtualDimension> Plan(RoomContour contour, PlanningOptions options, List<string> log) {
        contour.ThrowIfNull();
        options.ThrowIfNull();
        log.ThrowIfNull();

        var shape = new RoomShape(contour);
        var dimensions = CreateVirtualDimensions(contour, options, log);

        // У пилона оставляем по одному собственному размеру на ось: от него строится тип 3.
        // Сторона - верхняя/левая, если там хватает места под линию и текст, иначе - где места больше
        dimensions = RemoveDuplicates(
            dimensions, options, log, d => d.Kind == "Contour" && d.InvolvesInner,
            d => GetPylonSidePriority(contour, d, options));

        // Тип 3 строится от оставшихся размеров пилонов, чтобы встать на их линию.
        // Встречные размеры между двумя пилонами убираются сразу.
        dimensions.AddRange(CreateInternalDimensions(contour, dimensions, options, log));
        dimensions = RemoveDuplicates(dimensions, options, log, d => d.Kind == "Internal");

        dimensions = MergeIntoChains(dimensions, shape, options, log);

        // Дубли удаляются после объединения по одному правилу для размеров и цепочек:
        // все сегменты уже есть у другого размера и размерные линии ближе DuplicateMaxDistance
        dimensions = RemoveDuplicatesAfterChaining(dimensions, options, log);

        dimensions.AddRange(CreateOverallDimensions(contour, dimensions, options, log));

        // Размерная линия должна целиком лежать внутри помещения; общий размер еще и свободно от других размеров
        dimensions = KeepInsideRoom(shape, dimensions, options, log);

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
                    CreateAnchor(previous, line.Segment.Start.Dot(direction), loop.Index, perpendicular),
                    CreateAnchor(next, line.Segment.End.Dot(direction), loop.Index, perpendicular)
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
                var targetAnchor = CreateAnchor(
                    line,
                    line.Segment.Start.Dot(pylonDimension.Direction),
                    loopIndex,
                    pylonDimension.Perpendicular);
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
    /// Проверяет, что размерная линия целиком лежит внутри помещения и не пересекает контуры.
    /// Общий размер, кроме того, должен стоять свободно: с отступом от контура (со стороны текста - еще
    /// и на высоту текста) и не ближе шага яруса к параллельным размерам, иначе он режет их текст.
    /// Иначе он переносится в ближайшее свободное положение: у граней контура с отступом или следующим
    /// ярусом за параллельным размером; если такого нет - не ставится.
    /// Остальные размеры, вышедшие за пределы помещения (например, линия стены проходит через пилон у стены),
    /// переносятся так же; если свободного положения нет - остаются на месте с записью в журнал.
    /// </summary>
    private static List<VirtualDimension> KeepInsideRoom(
        RoomShape shape,
        List<VirtualDimension> dimensions,
        PlanningOptions options,
        List<string> log) {
        var result = new List<VirtualDimension>();
        var regular = dimensions.Where(d => d.Kind != "Overall").ToList();
        foreach(var dimension in regular) {
            result.Add(dimension);
            if(shape.Contains(dimension, dimension.LinePosition)) {
                continue;
            }
            var others = regular
                .Where(d => d != dimension && IsSameDirection(d, dimension) && Overlaps(d, dimension, options))
                .ToList();
            double? position = FindFreePosition(shape, dimension, others, options);
            if(position is null) {
                log.Add($"Размер {Names(dimension)}: размерная линия выходит за пределы помещения; "
                        + "свободного положения нет, оставлен на месте");
                continue;
            }
            log.Add($"Размер {Names(dimension)}: размерная линия выходит за пределы помещения; "
                    + $"перенесен на {Mm(Math.Abs(position.Value - dimension.LinePosition))} мм");
            dimension.LinePosition = position.Value;
        }

        foreach(var overall in dimensions.Where(d => d.Kind == "Overall")) {
            var parallel = result
                .Where(d => IsSameDirection(d, overall) && Overlaps(d, overall, options))
                .ToList();
            string conflict = GetPlacementConflict(shape, overall, overall.LinePosition, parallel, options);
            if(conflict is null) {
                result.Add(overall);
                continue;
            }

            double? found = FindFreePosition(shape, overall, parallel, options);
            if(found is null) {
                log.Add($"Общий размер {Names(overall)}: {conflict}; свободного положения внутри помещения нет, не создан");
                continue;
            }
            var neighbor = parallel
                .Where(d => Math.Abs(Math.Abs(d.LinePosition - found.Value) - options.TierStep) <= options.CoordinateTolerance)
                .OrderBy(d => d.Anchors.Count)
                .FirstOrDefault();
            log.Add($"Общий размер {Names(overall)}: {conflict}; перенесен на {Mm(Math.Abs(found.Value - overall.LinePosition))} мм"
                    + (neighbor is null ? " к грани контура" : $" ярусом за размером {Names(neighbor)}"));
            overall.LinePosition = found.Value;
            result.Add(overall);
        }
        return result;
    }

    /// <summary>
    /// Ближайшее к текущему свободное положение размерной линии. Кандидаты - у граней контура с отступом
    /// (и с местом под текст) и ярусом за параллельными размерами. Null, если свободного положения нет.
    /// </summary>
    private static double? FindFreePosition(
        RoomShape shape,
        VirtualDimension dimension,
        List<VirtualDimension> parallel,
        PlanningOptions options) {
        var perpendicular = dimension.Perpendicular;
        double textOffset = options.Offset + options.TextAllowance;
        var candidates = shape.Segments
            .SelectMany(seg => new[] { seg.Start.Dot(perpendicular), seg.End.Dot(perpendicular) })
            .SelectMany(v => new[] { v + options.Offset, v - options.Offset, v + textOffset, v - textOffset })
            .Concat(parallel.SelectMany(d => new[] { d.LinePosition + options.TierStep, d.LinePosition - options.TierStep }))
            .Distinct()
            .OrderBy(c => Math.Abs(c - dimension.LinePosition));
        foreach(double candidate in candidates) {
            if(GetPlacementConflict(shape, dimension, candidate, parallel, options) is null) {
                return candidate;
            }
        }
        return null;
    }

    /// <summary>
    /// Почему размерная линия в положении position мешает: null, если положение свободно.
    /// Проверяются выход за пределы помещения, отступ линии и текста от контура и шаг яруса до параллельных размеров.
    /// </summary>
    private static string GetPlacementConflict(
        RoomShape shape,
        VirtualDimension dimension,
        double position,
        List<VirtualDimension> parallel,
        PlanningOptions options) {
        if(!shape.Contains(dimension, position)) {
            return "размерная линия выходит за пределы помещения";
        }
        // Текст стоит с одной стороны линии: с этой стороны нужен отступ плюс высота текста
        int textSign = DimensionTextHelper.GetTextSide(dimension.Direction).Dot(dimension.Perpendicular) > 0 ? 1 : -1;
        double from = position - textSign * options.Offset;
        double to = position + textSign * (options.Offset + options.TextAllowance);
        // Полоса, ровно касающаяся грани контура, допустима
        double tolerance = options.CoordinateTolerance;
        if(!shape.ContainsBand(dimension, Math.Min(from, to) + tolerance, Math.Max(from, to) - tolerance)) {
            return "размерная линия или ее текст ближе отступа к контуру";
        }
        var near = parallel.FirstOrDefault(
            d => Math.Abs(d.LinePosition - position) < options.TierStep - options.CoordinateTolerance);
        if(near != null) {
            return $"размерная линия ближе шага яруса к размеру {Names(near)}";
        }
        return null;
    }

    /// <summary>
    /// Размеры перекрываются вдоль своего направления.
    /// </summary>
    private static bool Overlaps(VirtualDimension a, VirtualDimension b, PlanningOptions options) {
        return Math.Min(a.Max, b.Max) - Math.Max(a.Min, b.Min) > options.CoordinateTolerance;
    }

    /// <summary>
    /// Геометрия помещения на плане для проверки, лежит ли размерная линия внутри помещения.
    /// </summary>
    private class RoomShape {
        private readonly List<Point2D> _outerPolygon;
        private readonly List<List<Point2D>> _innerPolygons;

        public RoomShape(RoomContour contour) {
            Segments = contour.Loops.SelectMany(l => l.Lines.Select(x => x.Segment)).ToList();
            _outerPolygon = contour.Outer?.Lines.Select(l => l.Segment.Start).ToList() ?? new List<Point2D>();
            _innerPolygons = contour.Inners.Select(l => l.Lines.Select(x => x.Segment.Start).ToList()).ToList();
        }

        public List<Segment2D> Segments { get; }

        /// <summary>
        /// Размерная линия в положении position целиком внутри помещения и не пересекает контуры.
        /// </summary>
        public bool Contains(VirtualDimension dimension, double position) {
            if(_outerPolygon.Count < 3) {
                return true;
            }
            // Концы размерной линии лежат на плоскостях привязки, поэтому проверяем линию чуть короче
            double trim = Math.Min(_insideTrim, (dimension.Max - dimension.Min) / 4);
            var start = dimension.Direction * (dimension.Min + trim) + dimension.Perpendicular * position;
            var end = dimension.Direction * (dimension.Max - trim) + dimension.Perpendicular * position;
            if(Segments.Any(s => SegmentsIntersect(start, end, s.Start, s.End))) {
                return false;
            }
            var middle = (start + end) * 0.5;
            return IsPointInPolygon(middle, _outerPolygon)
                   && !_innerPolygons.Any(p => IsPointInPolygon(middle, p));
        }

        /// <summary>
        /// Полоса между положениями from и to (по оси Perpendicular) на длине размера целиком внутри помещения:
        /// ее не пересекает ни одна линия контура и в ней нет ни одного угла контура.
        /// </summary>
        public bool ContainsBand(VirtualDimension dimension, double from, double to) {
            if(_outerPolygon.Count < 3) {
                return true;
            }
            double trim = Math.Min(_insideTrim, (dimension.Max - dimension.Min) / 4);
            double alongMin = dimension.Min + trim;
            double alongMax = dimension.Max - trim;
            var direction = dimension.Direction;
            var perpendicular = dimension.Perpendicular;
            Point2D Corner(double along, double across) => direction * along + perpendicular * across;
            var corners = new[] {
                Corner(alongMin, from), Corner(alongMax, from), Corner(alongMax, to), Corner(alongMin, to)
            };
            for(int i = 0; i < corners.Length; i++) {
                var a = corners[i];
                var b = corners[(i + 1) % corners.Length];
                if(Segments.Any(s => SegmentsIntersect(a, b, s.Start, s.End))) {
                    return false;
                }
            }
            bool IsInside(Point2D point) {
                double along = point.Dot(direction);
                double across = point.Dot(perpendicular);
                return along > alongMin && along < alongMax && across > from && across < to;
            }
            if(Segments.Any(s => IsInside(s.Start) || IsInside(s.End))) {
                return false;
            }
            var middle = Corner((alongMin + alongMax) / 2, (from + to) / 2);
            return IsPointInPolygon(middle, _outerPolygon)
                   && !_innerPolygons.Any(p => IsPointInPolygon(middle, p));
        }
    }

    /// <summary>
    /// Пересекаются ли отрезки AB и CD (включая касание и наложение).
    /// </summary>
    private static bool SegmentsIntersect(Point2D a, Point2D b, Point2D c, Point2D d) {
        double o1 = (b - a).Cross(c - a);
        double o2 = (b - a).Cross(d - a);
        double o3 = (d - c).Cross(a - c);
        double o4 = (d - c).Cross(b - c);
        const double eps = 1e-9;
        if(Math.Abs(o1) < eps && Math.Abs(o2) < eps) {
            // Отрезки на одной прямой: пересекаются, если перекрываются проекции
            var direction = b - a;
            double t1 = (c - a).Dot(direction);
            double t2 = (d - a).Dot(direction);
            double length = direction.Dot(direction);
            return Math.Max(t1, t2) >= 0 && Math.Min(t1, t2) <= length;
        }
        // Концы CD по разные стороны от AB и концы AB по разные стороны от CD (касание считается пересечением)
        return o1 * o2 <= 0 && o3 * o4 <= 0;
    }

    /// <summary>
    /// Точка внутри многоугольника (метод луча).
    /// </summary>
    private static bool IsPointInPolygon(Point2D point, List<Point2D> polygon) {
        bool inside = false;
        for(int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++) {
            var pi = polygon[i];
            var pj = polygon[j];
            if((pi.Y > point.Y) != (pj.Y > point.Y)
               && point.X < (pj.X - pi.X) * (point.Y - pi.Y) / (pj.Y - pi.Y) + pi.X) {
                inside = !inside;
            }
        }
        return inside;
    }

    /// <summary>
    /// Крайние плоскости внешнего контура вдоль направления: линии, перпендикулярные направлению,
    /// и контурные точки. На одной координате берется самая длинная линия.
    /// </summary>
    private static (DimensionAnchor Min, DimensionAnchor Max)? GetExtremeAnchors(
        ContourLoop outer,
        Point2D direction,
        PlanningOptions options) {
        var perpendicular = direction.PerpendicularLeft();
        var candidates = new List<(DimensionAnchor Anchor, double Length)>();
        foreach(var element in outer.Elements) {
            if(element.Reference is null) {
                continue;
            }
            switch(element) {
                case ContourLine line when !line.IsArc
                                           && Math.Abs(line.Segment.Direction.Dot(direction)) <= _angleTolerance:
                    candidates.Add((CreateAnchor(
                        line, line.Segment.Start.Dot(direction), outer.Index, perpendicular), line.Segment.Length));
                    break;
                case ContourPoint point:
                    candidates.Add((CreateAnchor(
                        point, point.Location.Dot(direction), outer.Index, perpendicular), 0));
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
        Func<VirtualDimension, bool> scope,
        Func<VirtualDimension, double> priority = null) {
        // Из дублей остается первый: с большим приоритетом, затем верхний/левый
        priority ??= _ => 0;
        var ordered = dimensions.Where(scope)
            .OrderByDescending(priority)
            .ThenByDescending(d => d.LinePosition)
            .ToList();
        var kept = dimensions.Where(d => !scope(d)).ToList();
        int outsideCount = kept.Count;
        foreach(var dimension in ordered) {
            var duplicateOf = kept.Skip(outsideCount).FirstOrDefault(k => IsSameDirection(k, dimension)
                                                       && Math.Abs(k.Min - dimension.Min) <= options.CoordinateTolerance
                                                       && Math.Abs(k.Max - dimension.Max) <= options.CoordinateTolerance
                                                       && (options.PylonDuplicatesAcrossPylons || IsSameLoops(k, dimension)));
            if(duplicateOf != null) {
                log.Add($"Размер {Names(dimension)} ({Mm(dimension.Max - dimension.Min)} мм) - дубль {Names(duplicateOf)}, удален");
                continue;
            }
            kept.Add(dimension);
        }
        return kept;
    }

    /// <summary>
    /// Приоритет стороны собственного размера пилона. Если на стороне хватает места под отступ и текст,
    /// стороны равноценны (остается верхняя/левая), иначе выбирается сторона с большим свободным местом.
    /// </summary>
    private static double GetPylonSidePriority(RoomContour contour, VirtualDimension dimension, PlanningOptions options) {
        double free = GetFreeSpace(contour, dimension, options);
        return free >= options.Offset + options.TextAllowance - options.CoordinateTolerance ? double.MaxValue : free;
    }

    /// <summary>
    /// Свободное место перед образмериваемой линией: расстояние до ближайшей параллельной линии другой петли,
    /// которая перекрывает размер вдоль его направления.
    /// </summary>
    private static double GetFreeSpace(RoomContour contour, VirtualDimension dimension, PlanningOptions options) {
        var direction = dimension.Direction;
        var perpendicular = dimension.Perpendicular;
        double free = double.MaxValue;
        foreach(var loop in contour.Loops.Where(l => l.Index != dimension.SourceLoopIndex)) {
            foreach(var line in loop.Lines.Where(l => !l.IsArc)) {
                if(Math.Abs(line.Segment.Direction.Dot(perpendicular)) > _angleTolerance) {
                    continue;
                }
                double a = line.Segment.Start.Dot(direction);
                double b = line.Segment.End.Dot(direction);
                double overlap = Math.Min(Math.Max(a, b), dimension.Max) - Math.Max(Math.Min(a, b), dimension.Min);
                if(overlap <= options.CoordinateTolerance) {
                    continue;
                }
                double distance = (line.Segment.Start.Dot(perpendicular) - dimension.ContourPosition) * dimension.SideSign;
                if(distance > options.CoordinateTolerance) {
                    free = Math.Min(free, distance);
                }
            }
        }
        return free;
    }

    /// <summary>
    /// Размеры привязаны к одним и тем же петлям: собственные размеры одного пилона,
    /// размеры одного пилона до стены, встречные размеры между двумя пилонами.
    /// </summary>
    private static bool IsSameLoops(VirtualDimension a, VirtualDimension b) {
        var loopsA = a.Anchors.Select(x => x.LoopIndex).Distinct().OrderBy(x => x);
        var loopsB = b.Anchors.Select(x => x.LoopIndex).Distinct().OrderBy(x => x);
        return loopsA.SequenceEqual(loopsB);
    }

    /// <summary>
    /// Удаляет размер или цепочку, если все их сегменты уже есть у другого размера того же направления
    /// и размерные линии ближе DuplicateMaxDistance (0 - на любом расстоянии). Далеко разнесенные дубли остаются.
    /// Правило одно для одиночных размеров и цепочек. Из дублей остается тот, у кого больше сегментов;
    /// при равенстве - тот, у кого текст не обращен к стене (он ближе к стене, текст не зажат):
    /// на прямых стенах это нижний/правый; если и это равно - верхний/левый.
    /// </summary>
    private static List<VirtualDimension> RemoveDuplicatesAfterChaining(
        List<VirtualDimension> dimensions,
        PlanningOptions options,
        List<string> log) {
        var kept = new List<VirtualDimension>();
        var ordered = dimensions
            .OrderByDescending(d => d.Anchors.Count)
            .ThenBy(d => DimensionTextHelper.TextFacesContour(d.Direction, d.Side))
            .ThenByDescending(d => d.LinePosition);
        foreach(var dimension in ordered) {
            var covering = kept
                .Where(k => IsSameDirection(k, dimension) && ContainsAllSegments(k, dimension, options))
                .ToList();
            var duplicateOf = covering.FirstOrDefault(k => IsNearForDuplicate(k, dimension, options));
            if(duplicateOf != null) {
                log.Add($"Размер {Names(dimension)} ({SegmentsText(dimension)} мм) повторяет "
                        + $"{(duplicateOf.Anchors.Count > 2 ? "сегменты цепочки" : "размер")} {Names(duplicateOf)} "
                        + $"в {Mm(Math.Abs(duplicateOf.LinePosition - dimension.LinePosition))} мм, удален");
                continue;
            }
            if(covering.Count > 0) {
                var nearest = covering.OrderBy(k => Math.Abs(k.LinePosition - dimension.LinePosition)).First();
                log.Add($"Размер {Names(dimension)} ({SegmentsText(dimension)} мм) повторяет {Names(nearest)}, "
                        + $"но далеко ({Mm(Math.Abs(nearest.LinePosition - dimension.LinePosition))} мм "
                        + $"> {Mm(options.DuplicateMaxDistance)} мм), оставлен");
            }
            kept.Add(dimension);
        }
        return kept;
    }

    /// <summary>
    /// Размерные линии достаточно близко, чтобы считать размер дублем.
    /// </summary>
    private static bool IsNearForDuplicate(VirtualDimension a, VirtualDimension b, PlanningOptions options) {
        return options.DuplicateMaxDistance <= 0
               || Math.Abs(a.LinePosition - b.LinePosition) <= options.DuplicateMaxDistance + options.CoordinateTolerance;
    }

    /// <summary>
    /// Каждый сегмент размера part есть сегментом размера whole (те же соседние плоскости привязки).
    /// </summary>
    private static bool ContainsAllSegments(VirtualDimension whole, VirtualDimension part, PlanningOptions options) {
        var wholeSegments = GetSegments(whole);
        return GetSegments(part).All(
            p => wholeSegments.Any(w => Math.Abs(w.Min - p.Min) <= options.CoordinateTolerance
                                        && Math.Abs(w.Max - p.Max) <= options.CoordinateTolerance));
    }

    private static List<(double Min, double Max)> GetSegments(VirtualDimension dimension) {
        return dimension.Anchors
            .Zip(dimension.Anchors.Skip(1), (a, b) => (a.Coordinate, b.Coordinate))
            .ToList();
    }

    private static string SegmentsText(VirtualDimension dimension) {
        return string.Join(" | ", dimension.ExpectedSegments.Select(Mm));
    }

    /// <summary>
    /// Объединяет размеры с общей плоскостью в цепочки. Исходное расстояние между размерами не ограничено,
    /// ограничена длина вспомогательных линий: на линии цепочки ни одна не длиннее MaxWitnessLine.
    /// Линия цепочки не ближе к контуру, чем линия любого размера группы (иначе нарушится отступ и текст
    /// прижмется к стене), то есть на линии самого дальнего размера; при этом она должна лежать внутри помещения.
    /// </summary>
    private static List<VirtualDimension> MergeIntoChains(
        List<VirtualDimension> dimensions,
        RoomShape shape,
        PlanningOptions options,
        List<string> log) {
        if(!options.MergeChains) {
            log.Add("Объединение в цепочки выключено (MergeChains)");
            return dimensions;
        }
        var result = new List<VirtualDimension>();
        foreach(var component in GetConnectedComponents(dimensions, options)) {
            ProcessComponent(component, shape, options, log, result);
        }
        return result;
    }

    private static void ProcessComponent(
        List<VirtualDimension> component,
        RoomShape shape,
        PlanningOptions options,
        List<string> log,
        List<VirtualDimension> result) {
        if(component.Count == 1) {
            result.Add(component[0]);
            return;
        }

        // Кандидаты на линию цепочки - положения размеров группы, от ближайшего к стене.
        // Сторона размеров одна, поэтому "ближе к стене" - меньше положение вдоль стороны.
        int sideSign = component[0].SideSign;
        var candidates = component
            .Select(d => d.LinePosition)
            .Distinct()
            .OrderBy(p => p * sideSign)
            .ToList();

        double? bestPosition = null;
        List<VirtualDimension> bestViolators = null;
        foreach(double position in candidates) {
            // Отступ каждого размера от своего контура должен сохраниться
            if(component.Any(d => (position - d.LinePosition) * sideSign < -options.CoordinateTolerance)) {
                continue;
            }
            var probe = CreateChain(component, position, options);
            if(!shape.Contains(probe, position)) {
                continue;
            }
            var violators = component
                .Where(d => d.Anchors.Any(a => a.WitnessLength(position) > options.MaxWitnessLine))
                .ToList();
            if(bestViolators is null || violators.Count < bestViolators.Count) {
                bestPosition = position;
                bestViolators = violators;
            }
            if(violators.Count == 0) {
                break;
            }
        }

        if(bestPosition is null) {
            // Линию группы задают самые дальние от стены размеры. Если на ней цепочка выходит за пределы
            // помещения, они выводятся из группы, остальные собираются заново на более близкой линии
            double farthest = candidates[candidates.Count - 1];
            var farthestMembers = component
                .Where(d => Math.Abs(d.LinePosition - farthest) <= options.CoordinateTolerance)
                .ToList();
            if(farthestMembers.Count == component.Count) {
                log.Add($"Размеры {Names(component)} не объединены: на их линии цепочка выходит за пределы помещения");
                result.AddRange(component);
                return;
            }
            log.Add($"Размеры {Names(farthestMembers)} выведены из группы: на их линии цепочка группы "
                    + "выходит за пределы помещения, остальные собираются отдельно");
            var remaining = component.Except(farthestMembers).ToList();
            foreach(var subComponent in GetConnectedComponents(remaining, options)
                        .Concat(GetConnectedComponents(farthestMembers, options))) {
                ProcessComponent(subComponent, shape, options, log, result);
            }
            return;
        }
        if(bestViolators.Count == 0) {
            result.Add(CreateChain(component, bestPosition.Value, options));
            log.Add($"Цепочка из {Names(component)}, самая длинная вспомогательная линия "
                    + $"{Mm(result[result.Count - 1].MaxWitnessLength)} мм");
            return;
        }

        // Размеры с длинными вспомогательными линиями выводятся из группы, остальные собираются заново
        log.Add($"Размеры {Names(bestViolators)} выведены из группы: вспомогательные линии длиннее "
                + $"{Mm(options.MaxWitnessLine)} мм, остальные собираются отдельно");
        var rest = component.Except(bestViolators).ToList();
        foreach(var subComponent in GetConnectedComponents(rest, options)
                    .Concat(GetConnectedComponents(bestViolators, options))) {
            ProcessComponent(subComponent, shape, options, log, result);
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
    /// привязки и они не перекрываются. Расстояние между линиями не ограничено - его ограничивают
    /// вспомогательные линии при выборе положения цепочки.
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
        double overlap = Math.Min(a.Max, b.Max) - Math.Max(a.Min, b.Min);
        if(overlap > options.CoordinateTolerance) {
            return false;
        }
        return a.Anchors.Any(x => b.Anchors.Any(y => Math.Abs(x.Coordinate - y.Coordinate) <= options.CoordinateTolerance));
    }

    /// <summary>
    /// Привязка к элементу контура с его протяженностью поперек размера (для расчета вспомогательных линий).
    /// </summary>
    private static DimensionAnchor CreateAnchor(
        ContourElement element,
        double coordinate,
        int loopIndex,
        Point2D perpendicular) {
        double a;
        double b;
        switch(element) {
            case ContourLine line:
                a = line.Segment.Start.Dot(perpendicular);
                b = line.Segment.End.Dot(perpendicular);
                break;
            case ContourPoint point:
                a = b = point.Location.Dot(perpendicular);
                break;
            default:
                a = b = 0;
                break;
        }
        return new DimensionAnchor(element.Reference, coordinate, loopIndex, element.Index, a, b);
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
