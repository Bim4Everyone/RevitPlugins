using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitLintelsManager.Models;

/// <summary>Числовая точность, координаты и мировые границы Revit.</summary>
internal static class RevitGeometry {
    // Техническая точность в футах (0,001 мм), отдельно от ShortCurveTolerance и инженерных допусков.
    internal const double LengthEpsilon = 0.001 / 304.8;
    internal const double DirectionEpsilon = 1e-9;

    // Проверяет локальные границы, включая случай плоской геометрии.
    internal static bool HasValidBounds(XYZ minimum, XYZ maximum) {
        return IsFinitePoint(minimum) && IsFinitePoint(maximum)
            && minimum.X <= maximum.X && minimum.Y <= maximum.Y && minimum.Z <= maximum.Z
            && (minimum.X < maximum.X || minimum.Y < maximum.Y || minimum.Z < maximum.Z);
    }

    // Проверяет числовые координаты перед формированием рамки.
    internal static bool IsFinitePoint(XYZ point) {
        return point is not null && IsFiniteCoordinate(point.X) && IsFiniteCoordinate(point.Y)
            && IsFiniteCoordinate(point.Z);
    }

    // Исключает значения, которые не представляют координату модели.
    internal static bool IsFiniteCoordinate(double value) {
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }

    // Преобразует все восемь углов, поскольку две диагональные точки могут поменять порядок после поворота.
    internal static IEnumerable<XYZ> GetWorldCorners(BoundingBoxXYZ bounds) {
        var minimum = bounds.Min;
        var maximum = bounds.Max;
        var transform = bounds.Transform;
        foreach(double x in new[] { minimum.X, maximum.X }) {
            foreach(double y in new[] { minimum.Y, maximum.Y }) {
                foreach(double z in new[] { minimum.Z, maximum.Z }) {
                    yield return transform.OfPoint(new XYZ(x, y, z));
                }
            }
        }
    }

    // Копирует координаты API в независимый снимок.
    internal static GeometryPoint AsPoint(XYZ point) {
        return new GeometryPoint(point.X, point.Y, point.Z);
    }

    // Создаёт точку API для запроса внутри текущего поколения.
    internal static XYZ AsXyz(GeometryPoint point) {
        return new XYZ(point.X, point.Y, point.Z);
    }
}
