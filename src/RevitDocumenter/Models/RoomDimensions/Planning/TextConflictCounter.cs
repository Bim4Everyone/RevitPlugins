using System;
using System.Collections.Generic;
using System.Linq;

using RevitDocumenter.Models.RoomDimensions.Geometry;
using RevitDocumenter.Models.RoomDimensions.Layout;

namespace RevitDocumenter.Models.RoomDimensions.Planning;

/// <summary>
/// Модель текста размеров (D62): прямоугольник текста каждого сегмента и его конфликты
/// с текстами, размерными и вспомогательными линиями других размеров помещения.
/// Count - подсчет в журнал планирования; FindConflict - проверка положения при раскладке (D65).
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

    private const double _axisTolerance = 1e-9;

    public static void Count(List<VirtualDimension> dimensions, PlanningOptions options, List<string> log) {
        dimensions.ThrowIfNull();
        options.ThrowIfNull();
        log.ThrowIfNull();

        var texts = dimensions.SelectMany(d => GetTextBoxes(d, d.LinePosition, options)).ToList();

        // Не конфликт, а признак мелкого сегмента: задел под вынос текста (D3)
        foreach(var text in texts.Where(t => t.Width > t.Value + options.CoordinateTolerance)) {
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

    private static IEnumerable<TextBox> GetTextBoxes(VirtualDimension dimension, double position, PlanningOptions options) {
        int textSign = DimensionTextHelper.GetTextSide(dimension.Direction).Dot(dimension.Perpendicular) > 0 ? 1 : -1;
        // Прямоугольник ужат на допуск: касание текста и линии конфликтом не считается
        double tolerance = options.CoordinateTolerance;
        double near = position + textSign * options.TextGap;
        double far = position + textSign * (options.TextGap + options.TextSize);
        double from = Math.Min(near, far) + tolerance;
        double to = Math.Max(near, far) - tolerance;
        for(int i = 0; i + 1 < dimension.Anchors.Count; i++) {
            double start = dimension.Anchors[i].Coordinate;
            double end = dimension.Anchors[i + 1].Coordinate;
            double value = end - start;
            if(value <= tolerance) {
                continue;
            }
            double width = DimensionTextHelper.EstimateTextWidth(value, options.TextSize, options.TextWidthScale);
            double center = (start + end) / 2;
            double half = Math.Max(0, width / 2 - tolerance);
            yield return new TextBox(dimension, value, width, [
                ToWorld(dimension, center - half, from),
                ToWorld(dimension, center + half, from),
                ToWorld(dimension, center + half, to),
                ToWorld(dimension, center - half, to)
            ]);
        }
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

    private class TextBox {
        public TextBox(VirtualDimension dimension, double value, double width, List<Point2D> corners) {
            Dimension = dimension;
            Value = value;
            Width = width;
            Corners = corners;
        }

        public VirtualDimension Dimension { get; }

        /// <summary>
        /// Значение сегмента, футы.
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Оценка ширины текста, футы.
        /// </summary>
        public double Width { get; }

        public List<Point2D> Corners { get; }
    }
}
