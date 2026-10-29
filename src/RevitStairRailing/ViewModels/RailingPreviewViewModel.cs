using System.Collections.ObjectModel;
using System.Globalization;

namespace RevitStairRailing.ViewModels;

/// <summary>
/// Схема одного пролёта ограждения (развёртка): стойки, поручень, прутки, пластины, анкеры и размеры.
/// </summary>
internal class RailingPreviewViewModel {
    public const double CanvasWidth = 380;
    public const double CanvasHeight = 360;

    private const double LeftMargin = 70;
    private const double RightMargin = 70;
    private const double TopMargin = 30;
    private const double BottomMargin = 50;

    public ObservableCollection<PreviewLine> Lines { get; } = new();

    public ObservableCollection<PreviewLabel> Labels { get; } = new();

    /// <summary>
    /// Перерисовывает схему по текущим размерам (мм); неверные или пустые значения заменяются типовыми.
    /// </summary>
    public void Update(
        double postHeight,
        double postStep,
        IReadOnlyList<double> rodOffsets,
        double? plateLength,
        double? anchorLength) {

        Lines.Clear();
        Labels.Clear();
        postHeight = postHeight > 0 ? postHeight : 1000;
        postStep = postStep > 0 ? postStep : 600;
        double anchorDepth = anchorLength > 0 ? anchorLength.Value : 0;
        double scale = Math.Min(
            (CanvasWidth - LeftMargin - RightMargin) / postStep,
            (CanvasHeight - TopMargin - BottomMargin) / (postHeight + anchorDepth));
        double leftPostX = LeftMargin;
        double rightPostX = LeftMargin + postStep * scale;
        double handrailY = TopMargin;
        double bottomY = TopMargin + postHeight * scale;

        // траектория (край марша со смещением) и поручень с выпуском за стойки
        Lines.Add(new PreviewLine(leftPostX - 30, bottomY, rightPostX + 30, bottomY, 1, isDashed: true));
        Lines.Add(new PreviewLine(leftPostX - 20, handrailY, rightPostX + 20, handrailY, 5));
        foreach(double postX in new[] { leftPostX, rightPostX }) {
            Lines.Add(new PreviewLine(postX, handrailY, postX, bottomY, 4));
            if(plateLength > 0) {
                double halfPlate = Math.Max(plateLength.Value * scale / 2, 4);
                Lines.Add(new PreviewLine(postX - halfPlate, bottomY, postX + halfPlate, bottomY, 6));
            }

            if(anchorDepth > 0) {
                Lines.Add(new PreviewLine(postX, bottomY, postX, bottomY + anchorDepth * scale, 2));
            }
        }

        foreach(double rodOffset in rodOffsets.Where(offset => offset > 0 && offset < postHeight)) {
            double rodY = handrailY + rodOffset * scale;
            Lines.Add(new PreviewLine(leftPostX, rodY, rightPostX, rodY, 2));
            Labels.Add(new PreviewLabel(rightPostX + 24, rodY - 9, Format(rodOffset)));
        }

        Labels.Add(new PreviewLabel(4, (handrailY + bottomY) / 2 - 9, "H = " + Format(postHeight)));
        Labels.Add(new PreviewLabel((leftPostX + rightPostX) / 2 - 24, bottomY + 6 + anchorDepth * scale, "≤ " + Format(postStep)));
    }

    private static string Format(double value) {
        return value.ToString("0.#", CultureInfo.InvariantCulture);
    }
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
