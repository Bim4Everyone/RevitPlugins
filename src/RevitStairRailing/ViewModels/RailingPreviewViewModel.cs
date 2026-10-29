using System.Collections.ObjectModel;
using System.Globalization;

using RevitStairRailing.Models;

namespace RevitStairRailing.ViewModels;

/// <summary>
/// Схема ограждения (развёртка): наклонный участок марша и горизонтальный участок площадки —
/// стойки, поручень, прутки, крепление стоек и размеры. Элементы активного поля окна выделяются.
/// </summary>
internal class RailingPreviewViewModel {
    public const double CanvasWidth = 440;
    public const double CanvasHeight = 400;

    private const double LeftMargin = 70;
    private const double RightMargin = 60;
    private const double TopMargin = 40;
    private const double BottomMargin = 60;

    /// <summary>
    /// Уклон марша на схеме (подъём к длине в плане).
    /// </summary>
    private const double FlightSlope = 0.5;

    /// <summary>
    /// Пролётов на марше и на площадке на схеме.
    /// </summary>
    private const int FlightSpanCount = 2;
    private const int LandingSpanCount = 1;

    /// <summary>
    /// Условная глубина анкера на схеме, мм.
    /// </summary>
    private const double AnchorDepth = 90;

    private RailingPreviewSizes _sizes;
    private double _scale;
    private double _surfaceOriginY;

    public ObservableCollection<PreviewLine> Lines { get; } = new();

    public ObservableCollection<PreviewLabel> Labels { get; } = new();

    /// <summary>
    /// Перерисовывает схему по размерам (мм); неверные или пустые значения заменяются типовыми.
    /// </summary>
    public void Update(RailingPreviewSizes sizes) {
        _sizes = sizes;
        Lines.Clear();
        Labels.Clear();
        double postHeight = Positive(sizes.PostHeight, 1000);
        double postStep = Positive(sizes.PostStep, 600);
        double flightLength = postStep * FlightSpanCount;
        double flightRise = flightLength * FlightSlope;
        double totalLength = postStep * (FlightSpanCount + LandingSpanCount);
        _scale = Math.Min(
            (CanvasWidth - LeftMargin - RightMargin) / totalLength,
            (CanvasHeight - TopMargin - BottomMargin) / (flightRise + postHeight + AnchorDepth));
        _surfaceOriginY = TopMargin + (flightRise + postHeight) * _scale;

        // траектория: поверхность марша и площадки по оси ограждения
        var trajectory = new[] { (X: 0.0, Z: 0.0), (X: flightLength, Z: flightRise), (X: totalLength, Z: flightRise) };
        for(int index = 1; index < trajectory.Length; index++) {
            AddLine(trajectory[index - 1].X, trajectory[index - 1].Z, trajectory[index].X, trajectory[index].Z, 1,
                new[] { PreviewFields.Trajectory }, isDashed: true);
        }

        // поручень и прутки параллельно траектории
        AddPolyline(trajectory, postHeight, 5, PreviewFields.Handrail);
        foreach(double rodOffset in sizes.RodOffsets.Where(offset => offset > 0 && offset < postHeight)) {
            AddPolyline(trajectory, postHeight - rodOffset, 2, PreviewFields.Rods);
            AddLabel(totalLength, flightRise + postHeight - rodOffset, 8, -9, Format(rodOffset), PreviewFields.Rods);
        }

        // стойки: на марше (включая стойку в переломе) — крепление марша, дальше — крепление площадки
        bool isBasePlateLabeled = false;
        bool isCheekLabeled = false;
        for(int postNumber = 0; postNumber <= FlightSpanCount + LandingSpanCount; postNumber++) {
            double postX = postNumber * postStep;
            double surfaceZ = Math.Min(postX, flightLength) * FlightSlope;
            bool isFlightPost = postNumber <= FlightSpanCount;
            PostMount mount = isFlightPost ? sizes.FlightMount : sizes.LandingMount;
            string mountField = isFlightPost ? PreviewFields.FlightMount : PreviewFields.LandingMount;
            double postBottomZ = mount == PostMount.Top
                ? DrawBasePlate(postX, surfaceZ, mountField, ref isBasePlateLabeled)
                : DrawCheeks(postX, surfaceZ, mountField, ref isCheekLabeled);
            AddLine(postX, postBottomZ, postX, surfaceZ + postHeight, 4, new[] { PreviewFields.Post });
        }

        // размеры: высота — у первой стойки марша, шаг — на площадке
        AddDimension(-45 / _scale, 0, -45 / _scale, postHeight, "H = " + Format(postHeight), PreviewFields.PostHeight,
            isVertical: true);
        double landingStart = flightLength;
        AddDimension(landingStart, flightRise + postHeight + 30 / _scale, landingStart + postStep,
            flightRise + postHeight + 30 / _scale, "≤ " + Format(postStep), PreviewFields.PostStep, isVertical: false);
    }

    /// <summary>
    /// Опорная пластина на поверхности и анкеры под ней. Возвращает отметку низа стойки.
    /// </summary>
    private double DrawBasePlate(double postX, double surfaceZ, string mountField, ref bool isLabeled) {
        double plateLength = Positive(_sizes.PlateLength, 100);
        double plateThickness = Math.Max(Positive(_sizes.PlateThickness, 6), 2 / _scale);
        string[] plateFields = { PreviewFields.BasePlate, PreviewFields.Plates, mountField };
        AddRectangle(postX - plateLength / 2, surfaceZ, postX + plateLength / 2, surfaceZ + plateThickness, plateFields);
        if(_sizes.HasAnchors) {
            double anchorOffset = Math.Max(plateLength / 2 - Positive(_sizes.AnchorEdgeDistance, 20), 0);
            double[] anchorOffsets = _sizes.AnchorLayout == AnchorLayout.Triangle
                ? new[] { -anchorOffset, 0, anchorOffset }
                : new[] { -anchorOffset, anchorOffset };
            foreach(double offset in anchorOffsets) {
                AddLine(postX + offset, surfaceZ + plateThickness + 3 / _scale, postX + offset, surfaceZ - AnchorDepth, 2,
                    new[] { PreviewFields.Anchors, mountField });
            }
        }

        if(!isLabeled) {
            isLabeled = true;
            AddLabel(postX, surfaceZ - AnchorDepth, -30, 4,
                Format(plateLength) + "×" + Format(Positive(_sizes.PlateWidth, 0)) + "×"
                + Format(Positive(_sizes.PlateThickness, 0)),
                PreviewFields.BasePlate);
        }

        return surfaceZ + plateThickness;
    }

    /// <summary>
    /// Щёки по бокам низа стойки. Возвращает отметку низа стойки.
    /// </summary>
    private double DrawCheeks(double postX, double surfaceZ, string mountField, ref bool isLabeled) {
        double cheekHeight = Positive(_sizes.CheekHeight, 120);
        string[] cheekFields = { PreviewFields.Cheeks, PreviewFields.Plates, mountField };
        if(_sizes.CheekOrientation == CheekOrientation.AlongRailing) {
            // щека в плоскости ограждения видна целиком, стойка за ней
            double halfWidth = Positive(_sizes.CheekWidth, 60) / 2;
            AddRectangle(postX - halfWidth, surfaceZ, postX + halfWidth, surfaceZ + cheekHeight, cheekFields);
        } else {
            double gapHalf = Positive(_sizes.CheekGap, 30) / 2;
            double thickness = Math.Max(Positive(_sizes.CheekThickness, 6), 2 / _scale);
            AddRectangle(postX - gapHalf - thickness, surfaceZ, postX - gapHalf, surfaceZ + cheekHeight, cheekFields);
            AddRectangle(postX + gapHalf, surfaceZ, postX + gapHalf + thickness, surfaceZ + cheekHeight, cheekFields);
        }

        if(!isLabeled) {
            isLabeled = true;
            AddLabel(postX, surfaceZ, -30, 8,
                Format(Positive(_sizes.CheekWidth, 0)) + "×" + Format(cheekHeight) + "×"
                + Format(Positive(_sizes.CheekThickness, 0)),
                PreviewFields.Cheeks);
        }

        return surfaceZ;
    }

    /// <summary>
    /// Ломаная, параллельная траектории, на высоте <paramref name="height"/> над ней.
    /// </summary>
    private void AddPolyline((double X, double Z)[] trajectory, double height, double thickness, string field) {
        for(int index = 1; index < trajectory.Length; index++) {
            AddLine(trajectory[index - 1].X, trajectory[index - 1].Z + height, trajectory[index].X,
                trajectory[index].Z + height, thickness, new[] { field });
        }
    }

    /// <summary>
    /// Размер: размерная линия с засечками и текст.
    /// </summary>
    private void AddDimension(double x1, double z1, double x2, double z2, string text, string field, bool isVertical) {
        string[] fields = { field };
        AddLine(x1, z1, x2, z2, 1, fields);
        double tick = 5 / _scale;
        foreach((double X, double Z) end in new[] { (x1, z1), (x2, z2) }) {
            AddLine(end.X - tick, end.Z - tick, end.X + tick, end.Z + tick, 1, fields);
        }

        if(isVertical) {
            AddLabel(x1, (z1 + z2) / 2, -60, -9, text, field);
        } else {
            AddLabel((x1 + x2) / 2, z1, -22, -22, text, field);
        }
    }

    private void AddRectangle(double left, double bottom, double right, double top, string[] fields) {
        AddLine(left, top, right, top, 1.5, fields);
        AddLine(right, top, right, bottom, 1.5, fields);
        AddLine(right, bottom, left, bottom, 1.5, fields);
        AddLine(left, bottom, left, top, 1.5, fields);
    }

    /// <summary>
    /// Линия по координатам развёртки (мм: вдоль ограждения и по высоте).
    /// </summary>
    private void AddLine(double x1, double z1, double x2, double z2, double thickness, string[] fields,
        bool isDashed = false) {
        Lines.Add(new PreviewLine(ToCanvasX(x1), ToCanvasY(z1), ToCanvasX(x2), ToCanvasY(z2), thickness, isDashed,
            fields.Contains(_sizes.ActiveField)));
    }

    /// <summary>
    /// Подпись у точки развёртки со смещением в пикселях.
    /// </summary>
    private void AddLabel(double x, double z, double shiftX, double shiftY, string text, string field) {
        Labels.Add(new PreviewLabel(ToCanvasX(x) + shiftX, ToCanvasY(z) + shiftY, text, field == _sizes.ActiveField));
    }

    private double ToCanvasX(double x) {
        return LeftMargin + x * _scale;
    }

    private double ToCanvasY(double z) {
        return _surfaceOriginY - z * _scale;
    }

    private static double Positive(double? value, double defaultValue) {
        return value > 0 ? value.Value : defaultValue;
    }

    private static string Format(double value) {
        return value.ToString("0.#", CultureInfo.InvariantCulture);
    }
}

/// <summary>
/// Части схемы, которые выделяются при вводе в поле окна (значения — теги полей в окне).
/// </summary>
internal static class PreviewFields {
    public const string Post = "Post";
    public const string PostHeight = "PostHeight";
    public const string PostStep = "PostStep";
    public const string Handrail = "Handrail";
    public const string Rods = "Rods";
    public const string FlightMount = "FlightMount";
    public const string LandingMount = "LandingMount";
    public const string Plates = "Plates";
    public const string BasePlate = "BasePlate";
    public const string Anchors = "Anchors";
    public const string Cheeks = "Cheeks";
    public const string Trajectory = "Trajectory";
}

/// <summary>
/// Размеры для схемы, мм; null — значение не введено или не число.
/// </summary>
internal class RailingPreviewSizes {
    public double? PostHeight { get; set; }

    public double? PostStep { get; set; }

    public IReadOnlyList<double> RodOffsets { get; set; } = new List<double>();

    public PostMount FlightMount { get; set; }

    public PostMount LandingMount { get; set; }

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

    /// <summary>
    /// Часть схемы поля, в котором сейчас ввод (<see cref="PreviewFields"/>); null — ничего не выделять.
    /// </summary>
    public string ActiveField { get; set; }
}

/// <summary>
/// Линия схемы в координатах холста.
/// </summary>
internal class PreviewLine {
    public PreviewLine(
        double x1,
        double y1,
        double x2,
        double y2,
        double thickness,
        bool isDashed,
        bool isHighlighted) {

        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
        Thickness = thickness;
        IsDashed = isDashed;
        IsHighlighted = isHighlighted;
    }

    public double X1 { get; }

    public double Y1 { get; }

    public double X2 { get; }

    public double Y2 { get; }

    public double Thickness { get; }

    public bool IsDashed { get; }

    public bool IsHighlighted { get; }
}

/// <summary>
/// Подпись схемы: левый верхний угол текста в координатах холста.
/// </summary>
internal class PreviewLabel {
    public PreviewLabel(double x, double y, string text, bool isHighlighted) {
        X = x;
        Y = y;
        Text = text;
        IsHighlighted = isHighlighted;
    }

    public double X { get; }

    public double Y { get; }

    public string Text { get; }

    public bool IsHighlighted { get; }
}
