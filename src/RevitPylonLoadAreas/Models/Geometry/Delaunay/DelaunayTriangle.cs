using System;
using System.Collections.Generic;

namespace RevitPylonLoadAreas.Models.Geometry.Delaunay;

internal readonly struct DelaunayTriangle {
    /// <summary>
    /// Конструирует треугольник триангуляции Делоне
    /// </summary>
    /// <param name="v0">Индекс первой вершины в списке точек триангуляции Делоне</param>
    /// <param name="v1">Индекс второй вершины в списке точек триангуляции Делоне</param>
    /// <param name="v2">Индекс третьей вершины в списке точек триангуляции Делоне</param>
    /// <param name="points">Список точек триангуляции Делоне</param>
    public DelaunayTriangle(int v0, int v1, int v2, IList<XY> points) {
        if(points == null) {
            throw new ArgumentNullException(nameof(points));
        }

        V0 = v0;
        V1 = v1;
        V2 = v2;
        var a = points[v0];
        var b = points[v1];
        var c = points[v2];

        double d = 2 * ((a.X * (b.Y - c.Y)) + (b.X * (c.Y - a.Y)) + (c.X * (a.Y - b.Y)));
        double ux = (((a.X * a.X) + (a.Y * a.Y)) * (b.Y - c.Y)
                     + ((b.X * b.X) + (b.Y * b.Y)) * (c.Y - a.Y)
                     + ((c.X * c.X) + (c.Y * c.Y)) * (a.Y - b.Y))
                    / d; // x координата центра описанной окружности

        double uy = (((a.X * a.X) + (a.Y * a.Y)) * (c.X - b.X)
                     + ((b.X * b.X) + (b.Y * b.Y)) * (a.X - c.X)
                     + ((c.X * c.X) + (c.Y * c.Y)) * (b.X - a.X))
                    / d; // y координата центра описанной окружности

        Circumcenter = new XY(ux, uy);
        double radiusWithTolerance = (Circumcenter.DistanceTo(a) + XY.Tolerance);
        SqrDistanceLimit = radiusWithTolerance * radiusWithTolerance;
    }

    /// <summary>
    /// Индекс первой вершины в списке точек триангуляции Делоне
    /// </summary>
    public int V0 { get; }

    /// <summary>
    /// Индекс второй вершины в списке точек триангуляции Делоне
    /// </summary>
    public int V1 { get; }

    /// <summary>
    /// Индекс третьей вершины в списке точек триангуляции Делоне
    /// </summary>
    public int V2 { get; }

    /// <summary>
    /// Координаты описанной окружности
    /// </summary>
    public XY Circumcenter { get; }

    /// <summary>
    /// Квадрат радиуса описанной окружности с радиусом, увеличенным на <see cref="XY.Tolerance"/>
    /// </summary>
    private double SqrDistanceLimit { get; }

    /// <summary>
    /// Проверяет, находится ли точка внутри описанной окружности
    /// </summary>
    public bool CircumcircleContains(XY p) {
        return Circumcenter.SqrDistanceTo(p) <= SqrDistanceLimit;
    }
}
