using System.Collections.ObjectModel;
using System.Globalization;

using RevitStairRailing.Models;

namespace RevitStairRailing.ViewModels;

/// <summary>
/// Схема одного пролёта ограждения на марше (развёртка): стойки, поручень, прутки, крепление стоек и размеры.
/// </summary>
internal class RailingPreviewViewModel {
    public const double CanvasWidth = 380;
    public const double CanvasHeight = 380;

    private const double LeftMargin = 70;
    private const double RightMargin = 70;
    private const double TopMargin = 30;
    private const double BottomMargin = 70;

    /// <summary>
    /// Условная глубина анкера на схеме, мм.
    /// </summary>
    private const double AnchorDepth = 90;

    public ObservableCollection<PreviewLine> Lines { get; } = new();

    public ObservableCollection<PreviewLabel> Labels { get; } = new();

    /// <summary>
    /// Перерисовывает схему по размерам (мм); неверные или пустые значения заменяются типовыми.
    /// </summary>
    public void Update(RailingPreviewSizes sizes) {
        Lines.Clear();
        Labels.Clear();
        double postHeight = Positive(sizes.PostHeight, 1000);
        double postStep = Positive(sizes.PostStep, 600);
        double scale = Math.Min(
            (CanvasWidth - LeftMargin - RightMargin) / postStep,
            (CanvasHeight - TopMargin - BottomMargin) / postHeight);
        double leftPostX = LeftMargin;
        double rightPostX = LeftMargin + postStep * scale;
        double handrailY = TopMargin;
        double surfaceY = TopMargin + postHeight * scale;

        // траектория (поверхность марша по оси ограждения) и поручень с выпуском за стойки
        Lines.Add(new PreviewLine(leftPostX - 30, surfaceY, rightPostX + 30, surfaceY, 1, isDashed: true));
        Lines.Add(new PreviewLine(leftPostX - 20, handrailY, rightPostX + 20, handrailY, 5));
        foreach(double rodOffset in sizes.RodOffsets.Where(offset => offset > 0 && offset < postHeight)) {
            double rodY = handrailY + rodOffset * scale;
            Lines.Add(new PreviewLine(leftPostX, rodY, rightPostX, rodY, 2));
            Labels.Add(new PreviewLabel(rightPostX + 24, rodY - 9, Format(rodOffset)));
        }

        double postBottomY = surfaceY;
        foreach(double postX in new[] { leftPostX, rightPostX }) {
            postBottomY = sizes.Mount == PostMount.Top
                ? DrawBasePlate(sizes, postX, surfaceY, scale)
                : DrawCheeks(sizes, postX, surfaceY, scale);
            Lines.Add(new PreviewLine(postX, handrailY, postX, postBottomY, 4));
        }

        Labels.Add(new PreviewLabel(4, (handrailY + surfaceY) / 2 - 9, "H = " + Format(postHeight)));
        Labels.Add(new PreviewLabel((leftPostX + rightPostX) / 2 - 24, surfaceY + 8, "≤ " + Format(postStep)));
        string mountSize = sizes.Mount == PostMount.Top
            ? Format(Positive(sizes.PlateLength, 0)) + "×" + Format(Positive(sizes.PlateWidth, 0))
              + "×" + Format(Positive(sizes.PlateThickness, 0))
            : Format(Positive(sizes.CheekWidth, 0)) + "×" + Format(Positive(sizes.CheekHeight, 0))
              + "×" + Format(Positive(sizes.CheekThickness, 0));
        Labels.Add(new PreviewLabel(leftPostX - 40, surfaceY + 30 + AnchorDepth * scale, mountSize));
    }

    /// <summary>
    /// Опорная пластина на поверхности и анкеры под ней. Возвращает отметку низа стойки на холсте.
    /// </summary>
    private double DrawBasePlate(RailingPreviewSizes sizes, double postX, double surfaceY, double scale) {
        double halfLength = Math.Max(Positive(sizes.PlateLength, 100) * scale / 2, 4);
        double plateTopY = surfaceY - Math.Max(Positive(sizes.PlateThickness, 6) * scale, 2);
        AddRectangle(postX - halfLength, plateTopY, postX + halfLength, surfaceY);
        if(sizes.HasAnchors) {
            double anchorOffset = Math.Max(halfLength - Positive(sizes.AnchorEdgeDistance, 20) * scale, 0);
            var anchorXs = sizes.AnchorLayout == AnchorLayout.Triangle
                ? new[] { postX - anchorOffset, postX, postX + anchorOffset }
                : new[] { postX - anchorOffset, postX + anchorOffset };
            foreach(double anchorX in anchorXs) {
                Lines.Add(new PreviewLine(anchorX, plateTopY - 3, anchorX, surfaceY + AnchorDepth * scale, 2));
            }
        }

        return plateTopY;
    }

    /// <summary>
    /// Щёки по бокам низа стойки. Возвращает отметку низа стойки на холсте.
    /// </summary>
    private double DrawCheeks(RailingPreviewSizes sizes, double postX, double surfaceY, double scale) {
        double cheekTopY = surfaceY - Positive(sizes.CheekHeight, 120) * scale;
        if(sizes.CheekOrientation == CheekOrientation.AlongRailing) {
            // щека в плоскости ограждения видна целиком, стойка за ней
            double halfWidth = Positive(sizes.CheekWidth, 60) * scale / 2;
            AddRectangle(postX - halfWidth, cheekTopY, postX + halfWidth, surfaceY);
        } else {
            double gapHalf = Positive(sizes.CheekGap, 30) * scale / 2;
            double thickness = Math.Max(Positive(sizes.CheekThickness, 6) * scale, 2);
            AddRectangle(postX - gapHalf - thickness, cheekTopY, postX - gapHalf, surfaceY);
            AddRectangle(postX + gapHalf, cheekTopY, postX + gapHalf + thickness, surfaceY);
        }

        return surfaceY;
    }

    private void AddRectangle(double left, double top, double right, double bottom) {
        Lines.Add(new PreviewLine(left, top, right, top, 1.5));
        Lines.Add(new PreviewLine(right, top, right, bottom, 1.5));
        Lines.Add(new PreviewLine(right, bottom, left, bottom, 1.5));
        Lines.Add(new PreviewLine(left, bottom, left, top, 1.5));
    }

    private static double Positive(double? value, double defaultValue) {
        return value > 0 ? value.Value : defaultValue;
    }

    private static string Format(double value) {
        return value.ToString("0.#", CultureInfo.InvariantCulture);
    }
}

/// <summary>
/// Размеры для схемы пролёта, мм; null — значение не введено или не число.
/// </summary>
internal class RailingPreviewSizes {
    public double? PostHeight { get; set; }

    public double? PostStep { get; set; }

    public IReadOnlyList<double> RodOffsets { get; set; } = new List<double>();

    /// <summary>
    /// Крепление стоек на маршах.
    /// </summary>
    public PostMount Mount { get; set; }

    public double? PlateThickness { get; set; }

    public double? PlateLength { get; set; }

    public double? PlateWidth { get; set; }

    public bool HasAnchors { get; set; }

    public AnchorLayout AnchorLayout { get; set; }

    public double? AnchorEdgeDistance { get; set; }

    public double? CheekThickness { get; set; }

    public double? CheekWidth { get; set; }

    public double? CheekHeight { get; set; }

    public double? CheekGap { get; set; }

    public CheekOrientation CheekOrientation { get; set; }
}

/// <summary>
/// Линия схемы в координатах холста.
/// </summary>
internal class PreviewLine {
    public PreviewLine(double x1, double y1, double x2, double y2, double thickness, bool isDashed = false) {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
        Thickness = thickness;
        IsDashed = isDashed;
    }

    public double X1 { get; }

    public double Y1 { get; }

    public double X2 { get; }

    public double Y2 { get; }

    public double Thickness { get; }

    public bool IsDashed { get; }
}

/// <summary>
/// Подпись схемы: левый верхний угол текста в координатах холста.
/// </summary>
internal class PreviewLabel {
    public PreviewLabel(double x, double y, string text) {
        X = x;
        Y = y;
        Text = text;
    }

    public double X { get; }

    public double Y { get; }

    public string Text { get; }
}
