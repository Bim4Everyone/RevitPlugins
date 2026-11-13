using System;

namespace RevitPylonLoadAreas.Models.Geometry.Voronoi;

/// <summary>
/// Ячейка диаграммы Вороного
/// </summary>
internal sealed class VoronoiCell {
    /// <summary>
    /// Конструирует ячейку диаграммы Вороного
    /// </summary>
    /// <param name="polygon">Выпуклый многоугольник</param>
    /// <param name="site">Вершина ячейки</param>
    public VoronoiCell(Polygon2D polygon, VoronoiSite site) {
        Polygon = polygon ?? throw new ArgumentNullException(nameof(polygon));
        Site = site ?? throw new ArgumentNullException(nameof(site));
    }

    public Polygon2D Polygon { get; }

    public VoronoiSite Site { get; }
}
