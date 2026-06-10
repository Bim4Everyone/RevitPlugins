using System;
using System.Collections.Generic;

using RevitPylonLoadAreas.Models.Geometry;
using RevitPylonLoadAreas.Models.Geometry.Delaunay;

namespace RevitPylonLoadAreas.Services;

/// <summary>
/// Триангуляция Делоне методом Боуэра–Уотсона
/// </summary>
internal sealed class BowyerWatsonDelaunay {
    /// <summary>
    /// Все вершины триангуляции, первые 3 точки - вершины супертреугольника
    /// </summary>
    private readonly List<XY> _points = new();

    /// <summary>
    /// Текущий набор треугольников триангуляции. Индексы вершин = индексы точек в <see cref="_points"/>.
    /// </summary>
    private readonly List<DelaunayTriangle> _triangles = new();

    /// <summary>
    /// Текущий набор треугольников триангуляции. Индексы вершин = индексы точек в <see cref="_points"/>.
    /// </summary>
    public IList<DelaunayTriangle> Triangles => _triangles;

    /// <summary>
    /// Строит триангуляцию Делоне по набору точек
    /// </summary>
    /// <param name="sites">Точки, которые нужно триангулировать</param>
    /// <param name="floorBox">Прямоугольник, ограничивающий контур перекрытия</param>
    /// <returns>
    /// Массив индексов: для каждой исходной точки
    /// возвращается её позиция во внутреннем списке <see cref="_points"/>
    /// </returns>
    public int[] Triangulate(IList<XY> sites, BoundingBoxXY floorBox) {
        // 1. Создаём супертреугольник, накрывающий все точки — стартовая триангуляция.
        BuildSuperTriangle(sites, floorBox);
        int[] indices = new int[sites.Count];
        for(int i = 0; i < sites.Count; i++) {
            // Запоминаем позицию точки в общем списке вершин и добавляем её туда.
            indices[i] = _points.Count;
            _points.Add(sites[i]);
            // 2. Встраиваем точку в триангуляцию, локально перестраивая её.
            InsertPoint(indices[i]);
        }

        return indices;
    }

    /// <summary>
    /// Строит треугольник, внутри которого находятся все точки
    /// </summary>
    /// <param name="sites">Точки диаграммы Вороного</param>
    /// <param name="floorBox">Прямоугольник, ограничивающий контур перекрытия</param>
    private void BuildSuperTriangle(ICollection<XY> sites, BoundingBoxXY floorBox) {
        // построение равностороннего треугольника, вписанная окружность которого описывает с запасом прямоугольник, ограничивающий все точки
        _points.Clear();
        var bounds = new BoundingBoxXY([..sites, floorBox.Min, floorBox.Max]);
        var center = bounds.GetCenter();
        double r = bounds.GetDiagonalLength() / 2 + 1; // увеличенный на 1 радиус вписанной окружности треугольника
        double t = 6 * r / Math.Sqrt(3); // сторона равностороннего треугольника
        var a = new XY(center.X - t / 2, center.Y - r);
        var b = new XY(center.X, center.Y + 2 * r);
        var c = new XY(center.X + t / 2, center.Y - r);
        _points.Add(a);
        _points.Add(b);
        _points.Add(c);
        _triangles.Add(new DelaunayTriangle(0, 1, 2, _points));
    }

    /// <summary>
    /// Встраивает новую точку в триангуляцию, восстанавливая свойство Делоне.
    /// </summary>
    /// <param name="newIndex">Индекс новой точки в списке <see cref="_points"/></param>
    private void InsertPoint(int newIndex) {
        var newPoint = _points[newIndex];

        // поиск "плохих" треугольников, в чью описанную окружность попала новая точка
        var badTrianglesIndices = new List<int>();
        for(int i = 0; i < _triangles.Count; i++) {
            if(_triangles[i].CircumcircleContains(newPoint)) {
                badTrianglesIndices.Add(i);
            }
        }

        if(badTrianglesIndices.Count == 0) {
            return;
        }

        // "плохие" треугольники образуют полость, которую надо перестроить,
        // считаем, сколько плохих треугольников делят каждое ребро
        var edgeCounts = new Dictionary<DelaunayEdgeKey, int>();
        foreach(int i in badTrianglesIndices) {
            var triangle = _triangles[i];
            CountEdge(edgeCounts, triangle.V0, triangle.V1);
            CountEdge(edgeCounts, triangle.V1, triangle.V2);
            CountEdge(edgeCounts, triangle.V2, triangle.V0);
        }

        // удаляем все плохие треугольники из триангуляции
        for(int i = _triangles.Count - 1; i >= 0; i--) {
            if(badTrianglesIndices.Contains(i)) {
                _triangles.RemoveAt(i);
            }
        }

        // перестраиваем полость: соединяем каждое граничное ребро (встретилось ровно один раз) с новой точкой
        foreach(var edgeCount in edgeCounts) {
            if(edgeCount.Value == 1) {
                _triangles.Add(new DelaunayTriangle(edgeCount.Key.A, edgeCount.Key.B, newIndex, _points));
            }
        }
    }

    /// <summary>
    /// Увеличивает счётчик ребра (a, b)
    /// </summary>
    private void CountEdge(Dictionary<DelaunayEdgeKey, int> edgesCounts, int a, int b) {
        var edgeKey = new DelaunayEdgeKey(a, b);
        if(edgesCounts.TryGetValue(edgeKey, out int count)) {
            edgesCounts[edgeKey] = count + 1;
        } else {
            // ребро встречается впервые
            edgesCounts[edgeKey] = 1;
        }
    }
}
