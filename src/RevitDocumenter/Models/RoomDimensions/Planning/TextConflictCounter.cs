using System;
using System.Collections.Generic;
using System.Linq;

using RevitDocumenter.Models.RoomDimensions.Geometry;
using RevitDocumenter.Models.RoomDimensions.Layout;

namespace RevitDocumenter.Models.RoomDimensions.Planning;

/// <summary>
/// Модель текста размеров (D62): прямоугольник текста каждого сегмента и его конфликты
/// с текстами, размерными и вспомогательными линиями других размеров помещения.
/// Count - подсчет в журнал планирования; FindConflict - проверка положения при раскладке (D65);
/// Place - сдвиг текстов, конфликт которых раскладка не устранила (D68).
/// </summary>
/// <remarks>
/// Ширина текста оценивается по числу знаков значения в мм: до создания размера Revit ее не дает.
/// Текст стоит по центру сегмента со стороны DimensionTextHelper.GetTextSide.
/// Засечки, продление вспомогательных линий за размерную и зазор у элемента не учитываются.
/// </remarks>
internal static class TextConflictCounter {
    public const string TextText = "текст/текст";
    public const string TextLine = "текст/размерная линия";
    public const string TextWitness = "текст/вспомогательная линия";
    public const string WiderThanSegment = "текст шире сегмента";
    public const string TextDoor = "текст/дверное полотно";
    public const string TextDoorArc = "текст/дуга открывания";

    public const string ShiftAlong = "вдоль сегмента";
    public const string ShiftOtherSide = "на другую сторону линии";
    public const string ShiftOutside = "за сегмент";
    public const string ShiftOutsideOtherSide = "за сегмент на другую сторону линии";
    public const string ShiftLeader = "на выноске";
    public const string ShiftLeaderOtherSide = "на выноске на другой стороне линии";

    // Шаг сдвига текста вдоль сегмента и запас у нового положения, доля высоты текста (50 мм при 1:100)
    private const double _shiftStepRatio = 0.25;
    // Зазор текста, вынесенного за сегмент, от вспомогательной линии, доля высоты текста
    private const double _outsideGapRatio = 0.5;
    // Оценка кандидатов сдвига (D71): цены видов сдвига - доли высоты текста (при 1:100 высота 200 мм)
    private const double _outsidePenaltyRatio = 0.5;
    private const double _leaderPenaltyRatio = 1.0;
    private const double _otherSidePenaltyRatio = 0.75;
    private const double _leaderCrossPenaltyRatio = 2.0;
    // Цена единицы длины выноски и недостающего до высоты текста зазора с соседями
    private const double _leaderLengthWeight = 0.5;
    private const double _nearnessWeight = 2.0;
    // Дальние кандидаты за сегментом и на выноске: число шагов сдвига; уровни подъема выноски
    private const int _farSteps = 6;
    private const int _leaderLevels = 2;
    // Допуск касания текста линией, доля высоты текста (10 мм при 1:100), и проходы улучшения (D73)
    private const double _touchToleranceRatio = 0.05;
    private const int _improvementPasses = 3;
    // Отступ начала продолжения размерной линии от плоскости привязки при проверке контура, футы (1 мм)
    private const double _extensionStartOffset = 1.0 / 304.8;

    private const double _axisTolerance = 1e-9;

    public static void Count(List<VirtualDimension> dimensions, PlanningOptions options, List<string> log) {
        dimensions.ThrowIfNull();
        options.ThrowIfNull();
        log.ThrowIfNull();

        // Линия, лишь задевшая текст (допуск касания, D73), конфликтом не считается
        var texts = dimensions.SelectMany(d => GetTextBoxes(d, d.LinePosition, options, -GetTouchTolerance(options))).ToList();

        // Не конфликт, а признак мелкого сегмента, если текст не вынесен за сегмент (D3, D68)
        foreach(var text in texts.Where(t => t.Offset is null && t.Width > t.Value + options.CoordinateTolerance)) {
            text.Dimension.TextIssues.Add(WiderThanSegment);
            log.Add($"Текст {Describe(text)}: {WiderThanSegment} ({Mm(text.Width)} мм)");
        }

        // Пара текстов считается один раз и записывается за первым размером пары
        for(int i = 0; i < texts.Count; i++) {
            for(int j = i + 1; j < texts.Count; j++) {
                if(texts[i].Dimension != texts[j].Dimension && Intersects(texts[i].Corners, texts[j].Corners)) {
                    texts[i].Dimension.TextIssues.Add(TextText);
                    log.Add($"Конфликт {TextText}: {Describe(texts[i])} и {Describe(texts[j])}");
                }
            }
        }

        foreach(var text in texts) {
            string doorConflict = FindDoorConflict(text, options);
            if(doorConflict != null) {
                text.Dimension.TextIssues.Add(doorConflict.StartsWith(TextDoorArc, StringComparison.Ordinal)
                    ? TextDoorArc : TextDoor);
                log.Add($"Конфликт {doorConflict}: {Describe(text)}");
            }
            foreach(var other in dimensions.Where(d => d != text.Dimension)) {
                if(Intersects(text.Corners, GetDimensionLine(other, other.LinePosition))) {
                    text.Dimension.TextIssues.Add(TextLine);
                    log.Add($"Конфликт {TextLine}: {Describe(text)} и линия размера {Names(other)}");
                }
                var witness = GetWitnessLines(other, other.LinePosition, options)
                    .FirstOrDefault(w => Intersects(text.Corners, w.Line));
                if(witness.Line != null) {
                    text.Dimension.TextIssues.Add(TextWitness);
                    log.Add($"Конфликт {TextWitness}: {Describe(text)} и вспомогательная линия размера "
                            + $"{Names(other)} у {witness.Element}");
                }
            }
        }
    }

    /// <summary>
    /// Первый конфликт размера dimension, если поставить его размерную линию в положение position:
    /// его тексты с текстами и линиями размеров others и тексты others с его линиями. Null, если конфликтов нет.
    /// Текст шире сегмента конфликтом не считается.
    /// </summary>
    public static string FindConflict(
        VirtualDimension dimension,
        double position,
        IEnumerable<VirtualDimension> others,
        PlanningOptions options) {
        dimension.ThrowIfNull();
        others.ThrowIfNull();
        options.ThrowIfNull();

        var texts = GetTextBoxes(dimension, position, options).ToList();
        var line = GetDimensionLine(dimension, position);
        var witnesses = GetWitnessLines(dimension, position, options).Select(w => w.Line).ToList();
        foreach(var other in others.Where(d => d != dimension)) {
            var otherTexts = GetTextBoxes(other, other.LinePosition, options).ToList();
            if(texts.Any(t => otherTexts.Any(o => Intersects(t.Corners, o.Corners)))) {
                return $"{TextText} с размером {Names(other)}";
            }
            var otherLine = GetDimensionLine(other, other.LinePosition);
            var otherWitnesses = GetWitnessLines(other, other.LinePosition, options).Select(w => w.Line).ToList();
            if(texts.Any(t => Intersects(t.Corners, otherLine))
               || otherTexts.Any(o => Intersects(o.Corners, line))) {
                return $"{TextLine} с размером {Names(other)}";
            }
            if(texts.Any(t => otherWitnesses.Any(w => Intersects(t.Corners, w)))
               || otherTexts.Any(o => witnesses.Any(w => Intersects(o.Corners, w)))) {
                return $"{TextWitness} с размером {Names(other)}";
            }
        }
        return null;
    }

    /// <summary>
    /// Сдвиг текстов (D68), конфликт которых не устранил перенос размерных линий, и текстов шире сегмента.
    /// Кандидаты: вдоль сегмента, на другую сторону линии, за сегмент (только на свободном конце цепочки, D70),
    /// на выноске (D70), в том числе дальние. Годен кандидат без конфликтов с текстами (в т.ч. своими),
    /// выносками и линиями размеров помещения с запасом вдоль на шаг сдвига; текст и выноска - внутри
    /// помещения (fitsRoom, D69). Из годных берется самый дешевый (D71): цена вида сдвига, длина сдвига
    /// или выноски, штраф за близость к соседям и за пересечение выноской чужой линии.
    /// Тексты обходятся по порядку, сдвинутые учитываются дальше; затем до _improvementPasses проходов
    /// улучшения (D73). Текст, который линия лишь задевает (допуск касания), не сдвигается.
    /// </summary>
    public static void Place(
        List<VirtualDimension> dimensions,
        Func<IReadOnlyList<Point2D>, bool> fitsRoom,
        IReadOnlyList<Segment2D> contour,
        PlanningOptions options,
        List<string> log,
        ISet<VirtualDimension> movable = null) {
        dimensions.ThrowIfNull();
        fitsRoom.ThrowIfNull();
        contour.ThrowIfNull();
        options.ThrowIfNull();
        log.ThrowIfNull();

        // Первый проход: тексты по порядку, сдвинутые учитываются дальше
        var targets = new List<TextTarget>();
        foreach(var dimension in dimensions.Where(d => movable is null || movable.Contains(d))) {
            for(int i = 0; i + 1 < dimension.Anchors.Count; i++) {
                // При переносе цепочки сохраняем удачные места соседей. Переставляем лишь текст,
                // которому действительно помешала новая линия или ее вспомогательные линии.
                if(movable != null) {
                    dimension.TextOffsets.TryGetValue(i, out var currentOffset);
                    var current = GetTextBox(
                        dimension, i, dimension.LinePosition, currentOffset, -GetTouchTolerance(options), options);
                    if(current is null || ((currentOffset != null
                        || current.Width <= current.Value + options.CoordinateTolerance)
                       && FindCandidateConflict(current, dimensions, fitsRoom, options) is null)) {
                        continue;
                    }
                    dimension.TextOffsets.Remove(i);
                }
                string reason = FindPlacementReason(dimension, i, dimensions, options);
                if(reason is null) {
                    continue;
                }
                var target = new TextTarget(dimension, i, reason);
                (target.Offset, target.Crosses) = ChooseTextOffset(dimension, i, dimensions, fitsRoom, contour, options);
                if(target.Offset != null) {
                    dimension.TextOffsets[i] = target.Offset;
                }
                targets.Add(target);
            }
        }

        // Следующие проходы (D73): каждый текст заново выбирает место, когда остальные уже стоят;
        // текст, конфликт которого исчез после сдвига соседей, возвращается на место
        for(int pass = 0; pass < _improvementPasses; pass++) {
            bool changed = false;
            foreach(var target in targets.Where(t => !t.Restored)) {
                var dimension = target.Dimension;
                dimension.TextOffsets.Remove(target.Segment);
                if(FindPlacementReason(dimension, target.Segment, dimensions, options) is null) {
                    target.Offset = null;
                    target.Restored = true;
                    changed = true;
                    continue;
                }
                var (offset, crosses) = ChooseTextOffset(dimension, target.Segment, dimensions, fitsRoom, contour, options);
                if(offset != null && !SameOffset(offset, target.Offset)) {
                    target.Offset = offset;
                    target.Crosses = crosses;
                    changed = true;
                }
                if(target.Offset != null) {
                    dimension.TextOffsets[target.Segment] = target.Offset;
                }
            }
            if(!changed) {
                break;
            }
        }

        foreach(var target in targets) {
            var text = GetTextBox(target.Dimension, target.Segment, target.Dimension.LinePosition, null, 0, options);
            if(target.Restored) {
                log.Add($"Текст {Describe(text)} ({target.Reason}): на месте - конфликт ушел после сдвига соседей");
            } else if(target.Offset is null) {
                log.Add($"Текст {Describe(text)} ({target.Reason}): сдвиг не найден");
            } else {
                log.Add($"Текст {Describe(text)} ({target.Reason}): сдвинут {target.Offset.Mode}, "
                        + $"вдоль {Mm(target.Offset.Along)} мм"
                        + (target.Crosses ? ", выноска пересекает линию размера" : ""));
            }
        }
    }

    /// <summary>
    /// Остаточные проблемы текущей раскладки и цена сдвигов. Используется только при локальном переносе
    /// цепочки: сначала устраняем проблемы, затем предпочитаем короткие сдвиги и выноски (D75).
    /// </summary>
    internal static (int Problems, double Cost) GetLayoutScore(
        List<VirtualDimension> dimensions,
        Func<IReadOnlyList<Point2D>, bool> fitsRoom,
        PlanningOptions options) {
        int problems = 0;
        double cost = 0;
        foreach(var dimension in dimensions) {
            foreach(var text in GetTextBoxes(dimension, dimension.LinePosition, options, -GetTouchTolerance(options))) {
                bool wider = text.Offset is null && text.Width > text.Value + options.CoordinateTolerance;
                if(wider || FindCandidateConflict(text, dimensions, fitsRoom, options) != null) {
                    problems++;
                }
                if(text.Offset != null) {
                    cost += Math.Abs(text.Offset.Along) + Math.Abs(text.Offset.Across);
                    if(text.Leader != null) {
                        cost += options.TextSize * _leaderPenaltyRatio
                                + _leaderLengthWeight * (text.Leader[1] - text.Leader[0]).Length;
                    }
                }
            }
        }
        return (problems, cost);
    }

    /// <summary>
    /// Цепочки с оставшимся неразмещенным текстом. Успешные помещения новый этап не затрагивает.
    /// </summary>
    internal static List<VirtualDimension> GetUnplacedChains(
        List<VirtualDimension> dimensions,
        Func<IReadOnlyList<Point2D>, bool> fitsRoom,
        PlanningOptions options) {
        return dimensions.Where(d => d.Kind == "Chain" && GetTextBoxes(
                d, d.LinePosition, options, -GetTouchTolerance(options)).Any(t =>
                (t.Offset is null && t.Width > t.Value + options.CoordinateTolerance)
                || FindCandidateConflict(t, dimensions, fitsRoom, options) != null))
            .ToList();
    }

    /// <summary>
    /// Тексты в полосе переноса цепочки. Остальные сдвиги фиксированы во всех пробных раскладках.
    /// </summary>
    internal static HashSet<VirtualDimension> GetAffectedDimensions(
        VirtualDimension chain,
        double position,
        List<VirtualDimension> dimensions,
        PlanningOptions options) {
        var points = GetDimensionLine(chain, chain.LinePosition).Concat(GetDimensionLine(chain, position)).ToList();
        double margin = 2 * options.TextSize;
        double minX = points.Min(p => p.X) - margin;
        double maxX = points.Max(p => p.X) + margin;
        double minY = points.Min(p => p.Y) - margin;
        double maxY = points.Max(p => p.Y) + margin;
        bool Overlaps(IReadOnlyList<Point2D> polygon) => polygon.Min(p => p.X) <= maxX
            && polygon.Max(p => p.X) >= minX && polygon.Min(p => p.Y) <= maxY && polygon.Max(p => p.Y) >= minY;
        return new HashSet<VirtualDimension>(dimensions.Where(d => d == chain
            || GetTextBoxes(d, d.LinePosition, options)
                .Any(t => Overlaps(t.Corners) || (t.Leader != null && Overlaps(t.Leader)))));
    }

    /// <summary>
    /// Почему текст сегмента нужно сдвинуть с места по умолчанию: конфликт с чужими размерами глубже допуска
    /// касания (D73) или текст шире сегмента. Null - текст остается на месте.
    /// </summary>
    private static string FindPlacementReason(
        VirtualDimension dimension,
        int segment,
        IReadOnlyCollection<VirtualDimension> dimensions,
        PlanningOptions options) {
        var text = GetTextBox(dimension, segment, dimension.LinePosition, null, -GetTouchTolerance(options), options);
        if(text is null) {
            return null;
        }
        bool wider = text.Width > text.Value + options.CoordinateTolerance;
        return FindTextConflict(text, dimensions, false, options) ?? (wider ? WiderThanSegment : null);
    }

    /// <summary>
    /// Самый дешевый годный кандидат сдвига текста сегмента (D71) при текущих положениях остальных текстов.
    /// </summary>
    private static (TextOffset Offset, bool Crosses) ChooseTextOffset(
        VirtualDimension dimension,
        int segment,
        IReadOnlyCollection<VirtualDimension> dimensions,
        Func<IReadOnlyList<Point2D>, bool> fitsRoom,
        IReadOnlyList<Segment2D> contour,
        PlanningOptions options) {
        double step = options.TextSize * _shiftStepRatio;
        var text = GetTextBox(dimension, segment, dimension.LinePosition, null, 0, options);
        bool wider = text.Width > text.Value + options.CoordinateTolerance;
        var segments = GetTextBoxes(dimension, dimension.LinePosition, options).Select(t => t.Segment).ToList();
        bool first = segment == segments.First();
        bool last = segment == segments.Last();
        TextOffset best = null;
        double bestCost = double.MaxValue;
        bool bestCrosses = false;
        // D78: обход двери не должен переводить подпись через собственную размерную линию.
        // Revit может добавить выноску и пересечь ей текст. Сначала ищем место вдоль той же стороны;
        // если его нет, оставшаяся проблема передается локальному переносу цепочки (D75).
        bool keepTextSide = FindDoorConflict(text, options) != null;
        foreach(var (offset, kindCost) in GetTextOffsets(text, wider, first, last, step, options)) {
            if(keepTextSide && Math.Abs(offset.Across) > _axisTolerance) {
                continue;
            }
            var candidate = GetTextBox(dimension, segment, dimension.LinePosition, offset, step, options);
            if(FindCandidateConflict(candidate, dimensions, fitsRoom, options) != null
               || !ExtensionFitsRoom(text, offset, fitsRoom)) {
                continue;
            }
            var placed = GetTextBox(dimension, segment, dimension.LinePosition, offset, 0, options);
            bool crosses = placed.Leader != null && LeaderCrossesLines(placed, dimensions, options);
            double cost = kindCost
                          + (placed.Leader is null ? 0 : _leaderLengthWeight * (placed.Leader[1] - placed.Leader[0]).Length)
                          + (crosses ? options.TextSize * _leaderCrossPenaltyRatio : 0)
                          + GetNearnessPenalty(placed, dimensions, contour, options);
            if(cost < bestCost - _axisTolerance) {
                best = offset;
                bestCost = cost;
                bestCrosses = crosses;
            }
        }
        return (best, bestCrosses);
    }

    /// <summary>
    /// Текст за сегментом на линии (D73): продолжение размерной линии от конца сегмента до текста
    /// не проходит через конструкцию - под стеной или пилоном его не видно. Для остальных сдвигов - true.
    /// </summary>
    private static bool ExtensionFitsRoom(TextBox text, TextOffset offset, Func<IReadOnlyList<Point2D>, bool> fitsRoom) {
        if(offset.Leader || Math.Abs(offset.Along) <= text.Value / 2) {
            return true;
        }
        var dimension = text.Dimension;
        int sign = offset.Along > 0 ? 1 : -1;
        double start = dimension.Anchors[sign > 0 ? text.Segment + 1 : text.Segment].Coordinate;
        double middle = (dimension.Anchors[text.Segment].Coordinate + dimension.Anchors[text.Segment + 1].Coordinate) / 2;
        double near = middle + offset.Along - sign * text.Width / 2;
        // Начало чуть отступает от плоскости привязки: она лежит на контуре
        return fitsRoom([
            ToWorld(dimension, start + sign * _extensionStartOffset, dimension.LinePosition),
            ToWorld(dimension, near, dimension.LinePosition)
        ]);
    }

    private static bool SameOffset(TextOffset a, TextOffset b) {
        return a != null && b != null
               && Math.Abs(a.Along - b.Along) < _axisTolerance
               && Math.Abs(a.Across - b.Across) < _axisTolerance
               && a.Leader == b.Leader;
    }

    /// <summary>
    /// Допуск касания текста (D73): линия, зашедшая в оценочный прямоугольник текста не глубже,
    /// конфликтом не считается - ширина текста оценена приближенно.
    /// </summary>
    private static double GetTouchTolerance(PlanningOptions options) {
        return options.TextSize * _touchToleranceRatio;
    }

    /// <summary>
    /// Кандидаты сдвига текста (D68, D70, D71) с ценой вида сдвига. Тексту шире сегмента - только за сегмент
    /// или на выноске. За сегмент - только в сторону свободного конца цепочки (first, last): на линии
    /// соседнего сегмента текст читался бы как его размер. За сегмент и на выноске - и дальше, с шагом сдвига.
    /// Текст, который влезает в сегмент, на выноске может стоять ближе к сегменту (D72).
    /// </summary>
    private static IEnumerable<(TextOffset Offset, double Cost)> GetTextOffsets(
        TextBox text,
        bool wider,
        bool first,
        bool last,
        double step,
        PlanningOptions options) {
        double otherSide = -text.Sign * (2 * options.TextGap + options.TextSize);
        double sidePenalty = options.TextSize * _otherSidePenaltyRatio;
        if(!wider) {
            double room = (text.Value - text.Width) / 2;
            for(int k = 1; k * step <= room + options.CoordinateTolerance; k++) {
                foreach(double along in new[] { k * step, -k * step }) {
                    yield return (new TextOffset(along, 0, ShiftAlong), Math.Abs(along));
                }
            }
            yield return (new TextOffset(0, otherSide, ShiftOtherSide), sidePenalty);
            for(int k = 1; k * step <= room + options.CoordinateTolerance; k++) {
                foreach(double along in new[] { k * step, -k * step }) {
                    yield return (new TextOffset(along, otherSide, ShiftOtherSide), sidePenalty + Math.Abs(along));
                }
            }
        }
        double outside = text.Value / 2 + options.TextSize * _outsideGapRatio + text.Width / 2;
        foreach(double across in new[] { 0d, otherSide }) {
            foreach(int sign in new[] { 1, -1 }) {
                if(!(sign > 0 ? last : first)) {
                    continue;
                }
                for(int k = 0; k <= _farSteps; k++) {
                    yield return (
                        new TextOffset(sign * (outside + k * step), across, across == 0 ? ShiftOutside : ShiftOutsideOtherSide),
                        options.TextSize * _outsidePenaltyRatio + k * step + (across == 0 ? 0 : sidePenalty));
                }
            }
        }
        // Текст на выноске поднят над текстами соседних сегментов (уровень 1) или еще выше (уровень 2).
        // Текст, который влезает в сегмент, может стоять ближе: центром сразу за вспомогательной линией (D72);
        // текст шире сегмента - целиком за ней
        double leaderNear = wider ? outside : text.Value / 2 + step;
        int leaderSteps = (int) Math.Round((outside - leaderNear) / step) + _farSteps;
        for(int level = 1; level <= _leaderLevels; level++) {
            double lift = level * text.Sign * (options.TextSize + step);
            string levelName = level == 1 ? "" : $" (уровень {level})";
            foreach(double across in new[] { lift, otherSide - lift }) {
                foreach(int sign in new[] { 1, -1 }) {
                    for(int k = 0; k <= leaderSteps; k++) {
                        yield return (
                            new TextOffset(
                                sign * (leaderNear + k * step),
                                across,
                                (across == lift ? ShiftLeader : ShiftLeaderOtherSide) + levelName,
                                true),
                            options.TextSize * _leaderPenaltyRatio + (across == lift ? 0 : sidePenalty));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Первый конфликт кандидата сдвига: текст и выноска вне помещения, конфликт текста (FindTextConflict),
    /// выноска пересекает тексты. Null, если конфликтов нет. Пересечение выноской линий - не конфликт, а штраф.
    /// </summary>
    private static string FindCandidateConflict(
        TextBox text,
        IReadOnlyCollection<VirtualDimension> dimensions,
        Func<IReadOnlyList<Point2D>, bool> fitsRoom,
        PlanningOptions options) {
        if(!fitsRoom(text.Corners) || (text.Leader != null && !fitsRoom(text.Leader))) {
            return "вне помещения";
        }
        string conflict = FindTextConflict(text, dimensions, true, options);
        if(conflict != null || text.Leader is null) {
            return conflict;
        }
        foreach(var other in dimensions) {
            bool own = other == text.Dimension;
            if(GetTextBoxes(other, other.LinePosition, options)
               .Any(o => !(own && o.Segment == text.Segment) && Intersects(text.Leader, o.Corners))) {
                return $"выноска/текст с размером {Names(other)}";
            }
        }
        return null;
    }

    /// <summary>
    /// Выноска пересекает размерную или вспомогательную линию другого размера.
    /// Своя размерная линия - начало выноски, своя вспомогательная линия пересекается неизбежно.
    /// </summary>
    private static bool LeaderCrossesLines(TextBox text, IEnumerable<VirtualDimension> dimensions, PlanningOptions options) {
        return dimensions.Where(d => d != text.Dimension).Any(other =>
            Intersects(text.Leader, GetDimensionLine(other, other.LinePosition))
            || GetWitnessLines(other, other.LinePosition, options).Any(w => Intersects(text.Leader, w.Line)));
    }

    /// <summary>
    /// Штраф за близость текста к соседям ближе высоты текста (D71): тексты (и свои другие) и их выноски,
    /// размерные и вспомогательные линии других размеров, контур помещения.
    /// </summary>
    private static double GetNearnessPenalty(
        TextBox text,
        IEnumerable<VirtualDimension> dimensions,
        IReadOnlyList<Segment2D> contour,
        PlanningOptions options) {
        double clearance = options.TextSize;
        double penalty = 0;
        void Add(IReadOnlyList<Point2D> neighbor) {
            double distance = Distance(text.Corners, neighbor);
            if(distance < clearance) {
                penalty += _nearnessWeight * (clearance - distance);
            }
        }
        foreach(var other in dimensions) {
            bool own = other == text.Dimension;
            foreach(var otherText in GetTextBoxes(other, other.LinePosition, options)
                        .Where(o => !(own && o.Segment == text.Segment))) {
                Add(otherText.Corners);
                if(otherText.Leader != null) {
                    Add(otherText.Leader);
                }
            }
            if(own) {
                continue;
            }
            Add(GetDimensionLine(other, other.LinePosition));
            foreach(var witness in GetWitnessLines(other, other.LinePosition, options)) {
                Add(witness.Line);
            }
        }
        foreach(var segment in contour) {
            Add([segment.Start, segment.End]);
        }
        foreach(var door in options.DoorLeaves) {
            Add(door.Corners);
        }
        foreach(var arc in options.DoorOpeningArcs) {
            double distance = DistanceToDoorArc(text.Corners, arc, clearance);
            if(distance < clearance) {
                penalty += _nearnessWeight * (clearance - distance);
            }
        }
        return penalty;
    }

    /// <summary>
    /// Расстояние между выпуклыми многоугольниками или отрезками; 0, если пересекаются.
    /// </summary>
    private static double Distance(IReadOnlyList<Point2D> a, IReadOnlyList<Point2D> b) {
        if(Intersects(a, b)) {
            return 0;
        }
        double distance = double.MaxValue;
        for(int i = 0; i < (a.Count > 2 ? a.Count : 1); i++) {
            for(int j = 0; j < (b.Count > 2 ? b.Count : 1); j++) {
                distance = Math.Min(
                    distance,
                    SegmentDistance(a[i], a[(i + 1) % a.Count], b[j], b[(j + 1) % b.Count]));
            }
        }
        return distance;
    }

    private static double SegmentDistance(Point2D a, Point2D b, Point2D c, Point2D d) {
        double d1 = (b - a).Cross(c - a);
        double d2 = (b - a).Cross(d - a);
        double d3 = (d - c).Cross(a - c);
        double d4 = (d - c).Cross(b - c);
        if(d1 * d2 < 0 && d3 * d4 < 0) {
            return 0;
        }
        return Math.Min(
            Math.Min(PointSegmentDistance(a, c, d), PointSegmentDistance(b, c, d)),
            Math.Min(PointSegmentDistance(c, a, b), PointSegmentDistance(d, a, b)));
    }

    private static double PointSegmentDistance(Point2D point, Point2D start, Point2D end) {
        var segment = end - start;
        double lengthSquared = segment.Dot(segment);
        double t = lengthSquared <= 0 ? 0 : Math.Max(0, Math.Min(1, (point - start).Dot(segment) / lengthSquared));
        return (point - start - segment * t).Length;
    }

    /// <summary>
    /// Первый конфликт текста с текстами, размерными и вспомогательными линиями размеров.
    /// Со своим размером (includeOwn) - только с его другими текстами и вспомогательными линиями.
    /// </summary>
    private static string FindTextConflict(
        TextBox text,
        IEnumerable<VirtualDimension> dimensions,
        bool includeOwn,
        PlanningOptions options) {
        string doorConflict = FindDoorConflict(text, options);
        if(doorConflict != null) {
            return doorConflict;
        }
        foreach(var other in dimensions) {
            bool own = other == text.Dimension;
            if(own && !includeOwn) {
                continue;
            }
            var otherTexts = GetTextBoxes(other, other.LinePosition, options)
                .Where(o => !(own && o.Segment == text.Segment))
                .ToList();
            var otherText = otherTexts.FirstOrDefault(o => Intersects(text.Corners, o.Corners));
            if(otherText != null) {
                return $"{TextText} с {Describe(otherText)}";
            }
            var otherLeader = otherTexts.FirstOrDefault(o => o.Leader != null && Intersects(text.Corners, o.Leader));
            if(otherLeader != null) {
                return $"текст/выноска {Describe(otherLeader)}";
            }
            if(!own && Intersects(text.Corners, GetDimensionLine(other, other.LinePosition))) {
                return $"{TextLine} с размером {Names(other)}";
            }
            var witness = GetWitnessLines(other, other.LinePosition, options)
                .FirstOrDefault(w => Intersects(text.Corners, w.Line));
            if(witness.Line != null) {
                return $"{TextWitness} с размером {Names(other)} у {witness.Element}";
            }
        }
        return null;
    }

    private static string FindDoorConflict(TextBox text, PlanningOptions options) {
        double clearance = options.DoorTextClearance;
        var door = options.DoorLeaves.FirstOrDefault(d => Distance(text.Corners, d.Corners) <= clearance);
        if(door != null) {
            return $"{TextDoor} {door.DoorId} (с учетом запаса)";
        }
        var arc = options.DoorOpeningArcs
            .FirstOrDefault(a => DistanceToDoorArc(text.Corners, a, clearance) <= clearance);
        return arc is null ? null : $"{TextDoorArc} {arc.DoorId} (с учетом запаса)";
    }

    private static double DistanceToDoorArc(
        IReadOnlyList<Point2D> corners, DoorOpeningArcObstacle arc, double limit) {
        // Отсев далеких дуг: затем считаем расстояние до самой ломаной, не до ее области или хорды.
        double dx = Math.Max(0, Math.Max(arc.Min.X - corners.Max(p => p.X), corners.Min(p => p.X) - arc.Max.X));
        double dy = Math.Max(0, Math.Max(arc.Min.Y - corners.Max(p => p.Y), corners.Min(p => p.Y) - arc.Max.Y));
        if(dx * dx + dy * dy > limit * limit) {
            return double.MaxValue;
        }
        double result = double.MaxValue;
        foreach(var segment in arc.Segments) {
            result = Math.Min(result, Distance(corners, new[] { segment.Start, segment.End }));
            if(result <= _axisTolerance) {
                break;
            }
        }
        return result;
    }

    private static IEnumerable<TextBox> GetTextBoxes(
        VirtualDimension dimension,
        double position,
        PlanningOptions options,
        double margin = 0) {
        for(int i = 0; i + 1 < dimension.Anchors.Count; i++) {
            var offset = dimension.TextOffsets.TryGetValue(i, out var value) ? value : null;
            var text = GetTextBox(dimension, i, position, offset, margin, options);
            if(text != null) {
                yield return text;
            }
        }
    }

    /// <summary>
    /// Прямоугольник текста сегмента segment со сдвигом offset, расширенный вдоль на margin.
    /// Null для сегмента нулевой длины.
    /// </summary>
    private static TextBox GetTextBox(
        VirtualDimension dimension,
        int segment,
        double position,
        TextOffset offset,
        double margin,
        PlanningOptions options) {
        int textSign = DimensionTextHelper.GetTextSide(dimension.Direction).Dot(dimension.Perpendicular) > 0 ? 1 : -1;
        // Прямоугольник ужат на допуск: касание текста и линии конфликтом не считается
        double tolerance = options.CoordinateTolerance;
        double across = offset?.Across ?? 0;
        double near = position + textSign * options.TextGap + across;
        double far = position + textSign * (options.TextGap + options.TextSize) + across;
        double from = Math.Min(near, far) + tolerance;
        double to = Math.Max(near, far) - tolerance;
        double start = dimension.Anchors[segment].Coordinate;
        double end = dimension.Anchors[segment + 1].Coordinate;
        double value = end - start;
        if(value <= tolerance) {
            return null;
        }
        double width = DimensionTextHelper.EstimateTextWidth(value, options.TextSize, options.TextWidthScale);
        double middle = (start + end) / 2;
        double along = offset?.Along ?? 0;
        double center = middle + along;
        double half = Math.Max(0, width / 2 - tolerance) + margin;
        // Выноска: от середины сегмента на линии к ближнему нижнему углу текста
        double textHalf = half - margin;
        var leader = offset?.Leader == true
            ? new List<Point2D> {
                ToWorld(dimension, middle, position),
                ToWorld(
                    dimension,
                    along > 0 ? center - textHalf : center + textHalf,
                    Math.Abs(from - position) < Math.Abs(to - position) ? from : to)
            }
            : null;
        return new TextBox(dimension, segment, textSign, offset, value, width, [
            ToWorld(dimension, center - half, from),
            ToWorld(dimension, center + half, from),
            ToWorld(dimension, center + half, to),
            ToWorld(dimension, center - half, to)
        ], leader);
    }

    private static List<Point2D> GetDimensionLine(VirtualDimension dimension, double position) {
        return [
            ToWorld(dimension, dimension.Min, position),
            ToWorld(dimension, dimension.Max, position)
        ];
    }

    /// <summary>
    /// Вспомогательные линии: от ближайшего края элемента привязки до размерной линии.
    /// </summary>
    private static IEnumerable<(List<Point2D> Line, string Element)> GetWitnessLines(
        VirtualDimension dimension,
        double position,
        PlanningOptions options) {
        foreach(var anchor in dimension.Anchors) {
            if(anchor.WitnessLength(position) <= options.CoordinateTolerance) {
                continue;
            }
            double from = position < anchor.ExtentMin ? anchor.ExtentMin : anchor.ExtentMax;
            yield return (new List<Point2D> {
                ToWorld(dimension, anchor.Coordinate, from),
                ToWorld(dimension, anchor.Coordinate, position)
            }, $"{anchor.LoopIndex}:{anchor.ElementIndex}");
        }
    }

    /// <summary>
    /// Точка плана по координате вдоль размера и положению по оси Perpendicular.
    /// </summary>
    private static Point2D ToWorld(VirtualDimension dimension, double along, double across) {
        return dimension.Direction * along + dimension.Perpendicular * across;
    }

    /// <summary>
    /// Пересечение выпуклых многоугольников (отрезок - многоугольник из двух точек) по разделяющей оси.
    /// </summary>
    private static bool Intersects(IReadOnlyList<Point2D> a, IReadOnlyList<Point2D> b) {
        return !HasSeparatingAxis(a, b) && !HasSeparatingAxis(b, a);
    }

    private static bool HasSeparatingAxis(IReadOnlyList<Point2D> polygon, IReadOnlyList<Point2D> other) {
        for(int i = 0; i < polygon.Count; i++) {
            var edge = polygon[(i + 1) % polygon.Count] - polygon[i];
            if(edge.Length < _axisTolerance) {
                continue;
            }
            var axis = edge.PerpendicularLeft();
            double minA = polygon.Min(p => p.Dot(axis));
            double maxA = polygon.Max(p => p.Dot(axis));
            double minB = other.Min(p => p.Dot(axis));
            double maxB = other.Max(p => p.Dot(axis));
            if(maxA <= minB || maxB <= minA) {
                return true;
            }
        }
        return false;
    }

    private static string Describe(TextBox text) {
        return $"размер {Names(text.Dimension)} сегмент {Mm(text.Value)}";
    }

    private static string Names(VirtualDimension dimension) {
        return "[" + string.Join(", ", dimension.Sources) + "]";
    }

    private static string Mm(double feet) {
        return UnitUtilsHelper.ConvertFromInternalValue(feet).ToString("0.#");
    }

    /// <summary>
    /// Текст, который нужно сдвинуть, и выбранный для него сдвиг (D73).
    /// </summary>
    private class TextTarget {
        public TextTarget(VirtualDimension dimension, int segment, string reason) {
            Dimension = dimension;
            Segment = segment;
            Reason = reason;
        }

        public VirtualDimension Dimension { get; }
        public int Segment { get; }
        public string Reason { get; }
        public TextOffset Offset { get; set; }
        public bool Crosses { get; set; }

        /// <summary>
        /// Конфликт ушел после сдвига соседей: текст остается на месте.
        /// </summary>
        public bool Restored { get; set; }
    }

    private class TextBox {
        public TextBox(
            VirtualDimension dimension,
            int segment,
            int sign,
            TextOffset offset,
            double value,
            double width,
            List<Point2D> corners,
            List<Point2D> leader) {
            Dimension = dimension;
            Segment = segment;
            Sign = sign;
            Offset = offset;
            Value = value;
            Width = width;
            Corners = corners;
            Leader = leader;
        }

        public VirtualDimension Dimension { get; }

        public int Segment { get; }

        /// <summary>
        /// Сторона текста по умолчанию по оси Perpendicular размера: 1 или -1.
        /// </summary>
        public int Sign { get; }

        /// <summary>
        /// Сдвиг текста (D68); null - текст на месте Revit по умолчанию.
        /// </summary>
        public TextOffset Offset { get; }

        /// <summary>
        /// Значение сегмента, футы.
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Оценка ширины текста, футы.
        /// </summary>
        public double Width { get; }

        public List<Point2D> Corners { get; }

        /// <summary>
        /// Выноска текста (D70): отрезок от размерной линии до текста; null - без выноски.
        /// </summary>
        public List<Point2D> Leader { get; }
    }
}
