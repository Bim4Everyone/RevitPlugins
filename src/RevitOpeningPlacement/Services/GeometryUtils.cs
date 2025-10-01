using System;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Services;
/// <summary>
/// Класс предоставляет методы для работы с геометрией
/// </summary>
internal class GeometryUtils {
    public GeometryUtils() { }


    /// <summary>
    /// Возвращает точки ребер, образующих солид.
    /// </summary>
    /// <param name="solid">Солид</param>
    /// <param name="tessellationPoints">Количество точек тесселяции для изогнутых линий</param>
    /// <returns>Массив точек, образующих ребра солида</returns>
    public XYZ[] GetPoints(Solid solid, int tessellationPoints) {
        return solid.Edges
            .OfType<Edge>()
            .Select(edge => edge.AsCurve())
            .SelectMany(curve => curve is Line line
                    ? new XYZ[] { line.GetEndPoint(0) }
                    : curve.Tessellate().Take(tessellationPoints))
            .ToArray();
    }

    /// <summary>
    /// Находит модуль минимального расстояния от точек до плоскости
    /// </summary>
    /// <param name="plane">Плоскость</param>
    /// <param name="points">Массив точек</param>
    /// <returns>Модуль минимального расстояния в единицах Revit от точек до плоскости</returns>
    public double GetMinDistance(Plane plane, XYZ[] points) {
        return points.Select(p => Math.Abs(plane.Normal.DotProduct(p - plane.Origin))).Min();
    }
}
