using System;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Services {
    /// <summary>
    /// Класс предоставляет методы для работы с геометрией
    /// </summary>
    internal class GeometryUtils {
        public GeometryUtils() { }


        /// <summary>
        /// Возвращает точки ребер, образующих плоскую грань.
        /// </summary>
        /// <param name="planarFace">Плоская грань</param>
        /// <param name="tessellationPoints">Количество точек тесселяции для изогнутых линий</param>
        /// <returns>Массив точек, образующих ребра плоской грани</returns>
        public XYZ[] GetPoints(PlanarFace planarFace, int tessellationPoints) {
            return planarFace.EdgeLoops
                .OfType<EdgeArray>()
                .SelectMany(arr => arr
                    .OfType<Edge>()
                    .Select(edge => edge.AsCurve())
                    .SelectMany(curve => curve is Line line
                        ? new XYZ[] { line.GetEndPoint(0) }
                        : curve.Tessellate().Take(tessellationPoints)))
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
}
