using System;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.RoomDimensions.Geometry;

namespace RevitDocumenter.Models.RoomDimensions.Layout;

/// <summary>
/// Определяет, с какой стороны размерной линии Revit ставит текст, и сколько места текст занимает.
/// </summary>
internal static class DimensionTextHelper {
    private const double _axisTolerance = 1e-6;

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
        dimensionType.ThrowIfNull();
        double textSize = dimensionType.get_Parameter(BuiltInParameter.TEXT_SIZE)?.AsDouble() ?? 0;
        double textGap = dimensionType.get_Parameter(BuiltInParameter.TEXT_DIST_TO_LINE)?.AsDouble() ?? 0;
        return (textSize + textGap) * viewScale;
    }
}
