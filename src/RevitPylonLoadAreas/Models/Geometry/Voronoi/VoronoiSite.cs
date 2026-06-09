using System;

using Autodesk.Revit.DB;

namespace RevitPylonLoadAreas.Models.Geometry.Voronoi;

/// <summary>
/// Вершина ячейки диаграммы Вороного
/// </summary>
internal sealed class VoronoiSite {
    /// <summary>
    /// Конструирует вершину ячейки по точке и Id элемента Revit
    /// </summary>
    /// <param name="point">Координаты на плоскости</param>
    /// <param name="element">Элемента, который расположен в этой точке</param>
    public VoronoiSite(XY point, Element element) {
        Point = point;
        Element = element ?? throw new ArgumentNullException(nameof(element));
    }

    public XY Point { get; }

    public Element Element { get; }
}
