using System;
using System.Globalization;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.RoomDimensions.Geometry;

namespace RevitDocumenter.Models.RoomDimensions.Layout;

/// <summary>
/// Определяет, с какой стороны размерной линии Revit ставит текст, и сколько места текст занимает.
/// </summary>
internal static class DimensionTextHelper {
    private const double _axisTolerance = 1e-6;

    // Ширина цифры в долях высоты текста при коэффициенте ширины 1 - оценка, Revit ее до создания не дает
    private const double _digitWidthRatio = 0.7;

    /// <summary>
    /// Единичный вектор от размерной линии к тексту.
    /// Revit читает размер слева направо (вертикальный - снизу вверх) и ставит текст над линией:
    /// у горизонтальных размеров текст сверху, у вертикальных - слева.
    /// </summary>
    public static Point2D GetTextSide(Point2D dimensionDirection) {
        var direction = dimensionDirection.Normalize();
        if(direction.X < -_axisTolerance
           || (Math.Abs(direction.X) <= _axisTolerance && direction.Y < 0)) {
            direction = -direction;
        }
        return direction.PerpendicularLeft();
    }

    /// <summary>
    /// Текст размера, отложенного от контура по нормали внутрь, окажется между линией и контуром.
    /// </summary>
    public static bool TextFacesContour(Point2D dimensionDirection, Point2D inwardNormal) {
        return GetTextSide(dimensionDirection).Dot(inwardNormal) < 0;
    }

    /// <summary>
    /// Высота текста и его зазор от размерной линии в единицах модели (футы).
    /// </summary>
    public static double GetTextAllowance(DimensionType dimensionType, int viewScale) {
        return GetTextSize(dimensionType, viewScale) + GetTextGap(dimensionType, viewScale);
    }

    /// <summary>
    /// Высота текста в единицах модели (футы).
    /// </summary>
    public static double GetTextSize(DimensionType dimensionType, int viewScale) {
        dimensionType.ThrowIfNull();
        return (dimensionType.get_Parameter(BuiltInParameter.TEXT_SIZE)?.AsDouble() ?? 0) * viewScale;
    }

    /// <summary>
    /// Зазор текста от размерной линии в единицах модели (футы).
    /// </summary>
    public static double GetTextGap(DimensionType dimensionType, int viewScale) {
        dimensionType.ThrowIfNull();
        return (dimensionType.get_Parameter(BuiltInParameter.TEXT_DIST_TO_LINE)?.AsDouble() ?? 0) * viewScale;
    }

    /// <summary>
    /// Коэффициент ширины текста типоразмера; 1, если не задан.
    /// </summary>
    public static double GetTextWidthScale(DimensionType dimensionType) {
        dimensionType.ThrowIfNull();
        double widthScale = dimensionType.get_Parameter(BuiltInParameter.TEXT_WIDTH_SCALE)?.AsDouble() ?? 0;
        return widthScale > 0 ? widthScale : 1;
    }

    /// <summary>
    /// Оценка ширины текста значения (целые мм) в единицах модели: число знаков * ширина цифры.
    /// </summary>
    public static double EstimateTextWidth(double valueFeet, double textSize, double widthScale) {
        string text = Math.Round(UnitUtilsHelper.ConvertFromInternalValue(valueFeet))
            .ToString(CultureInfo.InvariantCulture);
        return text.Length * textSize * _digitWidthRatio * widthScale;
    }
}
