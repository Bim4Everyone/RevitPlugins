using System;
using System.Collections.Generic;
using System.Linq;

using RevitPylonLoadAreas.Models;
using RevitPylonLoadAreas.Models.Geometry;
using RevitPylonLoadAreas.Models.Geometry.Voronoi;

namespace RevitPylonLoadAreas.Services;

/// <summary>
/// Строит диаграмму Вороного на основе триангуляции Делоне.
/// <para>
/// Диаграмма Вороного двойственна триангуляции Делоне: ячейка вокруг точки (site) образована
/// центрами описанных окружностей всех треугольников Делоне, которым принадлежит эта точка.
/// Если упорядочить эти центры по углу вокруг точки, они образуют границу выпуклой ячейки.
/// </para>
/// </summary>
internal sealed class VoronoiBuilder {
    private readonly RevitRepository _repo;

    public VoronoiBuilder(RevitRepository repo) {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
    }
    
    /// <summary>
    /// Строит ячейки диаграммы Вороного для заданных точек.
    /// </summary>
    /// <param name="sites">Исходные точки (центры ячеек) с привязкой к элементам Revit</param>
    /// <param name="floorBox">Прямоугольник, ограничивающий контур перекрытия</param>
    /// <returns>Набор построенных ячеек Вороного</returns>
    public ICollection<VoronoiCell> Build(IList<VoronoiSite> sites, BoundingBoxXY floorBox) {
        if(sites == null) {
            throw new ArgumentNullException(nameof(sites));
        }

        if(sites.Count == 1) {
            return [new VoronoiCell(floorBox.AsPolygon2D(), sites[0])];
        }

        if(sites.Count == 0) {
            return [];
        }

        var cells = new List<VoronoiCell>(sites.Count);
        var sitePoints = sites.Select(s => s.Point).ToArray();
        var delaunay = new BowyerWatsonDelaunay();
        int[] siteIndices = delaunay.Triangulate(sitePoints, floorBox);

        // для каждого индекса точки находим индексы примыкающих к ней треугольников
        var trianglesBySite = new Dictionary<int, List<int>>();
        for(int t = 0; t < delaunay.Triangles.Count; t++) {
            var tri = delaunay.Triangles[t];
            AddTriangleToSite(trianglesBySite, tri.V0, t);
            AddTriangleToSite(trianglesBySite, tri.V1, t);
            AddTriangleToSite(trianglesBySite, tri.V2, t);
        }

        // строим ячейки Вороного
        for(int s = 0; s < sites.Count; s++) {
            int siteIndex = siteIndices[s];
            IList<XY> cellRing;
            if(trianglesBySite.TryGetValue(siteIndex, out var triangleIndices)) {
                var ordered = OrderCircumcentersAroundSite(delaunay, triangleIndices, sitePoints[s]);
                if(ordered.Count >= 3) {
                    cellRing = ordered;
                } else {
                    continue;
                }
            } else {
                continue;
            }

            cells.Add(new VoronoiCell(new Polygon2D(cellRing), sites[s]));
        }

        return cells;
    }

    /// <summary>
    /// Добавляет треугольник в список индексов треугольников, примыкающих к данной вершине (точке)
    /// </summary>
    /// <param name="trianglesBySite">Ключ - индекс точки, значение - список индексов треугольников, примыкающих к ней</param>
    /// <param name="siteIndex">Индекс точки</param>
    /// <param name="triangleIndex">Индекс треугольника</param>
    private void AddTriangleToSite(Dictionary<int, List<int>> trianglesBySite, int siteIndex, int triangleIndex) {
        if(!trianglesBySite.TryGetValue(siteIndex, out var trianglesIndices)) {
            trianglesIndices = [];
            trianglesBySite[siteIndex] = trianglesIndices;
        }

        trianglesIndices.Add(triangleIndex);
    }

    /// <summary>
    /// Упорядочивает центры описанных окружностей треугольников по углу вокруг точки,
    /// формируя замкнутый контур ячейки Вороного (вершины идут против часовой стрелки).
    /// </summary>
    /// <param name="delaunay">Триангуляция Делоне, из которой берутся центры окружностей</param>
    /// <param name="triangleIndices">Индексы треугольников, примыкающих к точке</param>
    /// <param name="site">Центр ячейки, относительно которой считается угол</param>
    /// <returns>Вершины контура ячейки в угловом порядке без дубликатов</returns>
    private List<XY> OrderCircumcentersAroundSite(
        BowyerWatsonDelaunay delaunay,
        List<int> triangleIndices,
        XY site) {
        var entries = new List<(double Angle, XY Center)>(triangleIndices.Count);
        foreach(int ti in triangleIndices) {
            var tri = delaunay.Triangles[ti];
            var center = tri.Circumcenter;
            double angle = Math.Atan2(center.Y - site.Y, center.X - site.X);
            entries.Add((angle, center));
        }

        entries.Sort((a, b) => a.Angle.CompareTo(b.Angle));
        var ordered = new List<XY>(entries.Count);
        foreach(var e in entries) {
            if(ordered.Count > 0
               && IsTooClose(ordered[ordered.Count - 1], e.Center)) {
                // пропускаем дублирующиеся точки, идущие подряд
                continue;
            }

            ordered.Add(e.Center);
        }

        if(ordered.Count > 1
           && IsTooClose(ordered[0], ordered[ordered.Count - 1])) {
            // удаляем дубль первой и последней точек
            ordered.RemoveAt(ordered.Count - 1);
        }

        return ordered;
    }

    private bool IsTooClose(XY first, XY second) {
        return first.IsAlmostEqual(second) || first.DistanceTo(second) <= _repo.Application.ShortCurveTolerance;
    }
}
