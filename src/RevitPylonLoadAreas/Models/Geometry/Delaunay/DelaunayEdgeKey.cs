using System;

namespace RevitPylonLoadAreas.Models.Geometry.Delaunay;

/// <summary>
/// Ключ ребра триангуляции Делоне: упорядоченная пара индексов вершин
/// </summary>
internal readonly struct DelaunayEdgeKey : IEquatable<DelaunayEdgeKey> {
    /// <summary>
    /// Создаёт ключ ребра, нормализуя порядок вершин (A &lt;= B)
    /// </summary>
    /// <param name="a">Индекс вершины A</param>
    /// <param name="b">Индекс вершины B</param>
    public DelaunayEdgeKey(int a, int b) {
        if(a < b) {
            A = a;
            B = b;
        } else {
            A = b;
            B = a;
        }
    }

    /// <summary>
    /// Меньший индекс вершины ребра
    /// </summary>
    public int A { get; }

    /// <summary>
    /// Больший индекс вершины ребра
    /// </summary>
    public int B { get; }

    public bool Equals(DelaunayEdgeKey other) {
        return A == other.A && B == other.B;
    }

    public override bool Equals(object obj) {
        return obj is DelaunayEdgeKey k && Equals(k);
    }

    public override int GetHashCode() {
        return unchecked((A * 397) ^ B);
    }
}
