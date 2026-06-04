using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitPylonLoadAreas.Models.Geometry;

/// <summary>
/// Грузовая площадь пилона. Может состоять из нескольких частей,
/// если ячейка Вороного пересекает дыры в плите или разорванные части плиты.
/// </summary>
internal sealed class LoadArea {
    public LoadArea(ElementId pylonId, IReadOnlyList<Polygon2D> pieces) {
        PylonId = pylonId;
        Pieces = pieces ?? new List<Polygon2D>();
    }

    /// <summary>
    /// Id экземпляра колонны (FamilyInstance).
    /// </summary>
    public ElementId PylonId { get; }

    /// <summary>
    /// Полигональные части грузовой площади. Каждая часть может иметь дыры.
    /// Обычно один элемент; больше одного — если ячейка раздроблена при клиппинге.
    /// </summary>
    public IReadOnlyList<Polygon2D> Pieces { get; }

    /// <summary>
    /// Суммарная площадь по всем частям, в футах^2.
    /// </summary>
    public double TotalArea => Pieces.Sum(p => p.Area);
}

/// <summary>
/// Результат работы пайплайна.
/// </summary>
internal sealed class PylonLoadAreasResult {
    public PylonLoadAreasResult(
        IReadOnlyDictionary<ElementId, LoadArea> byPylon,
        IReadOnlyList<Polygon2D> unionedFloorOutline,
        double slabElevation) {
        ByPylon = byPylon;
        UnionedFloorOutline = unionedFloorOutline;
        SlabElevation = slabElevation;
    }

    /// <summary>
    /// Грузовые площади, ключ — Id пилона (FamilyInstance).
    /// </summary>
    public IReadOnlyDictionary<ElementId, LoadArea> ByPylon { get; }

    /// <summary>
    /// Объединенный контур плиты (внешние области с дырами).
    /// Может содержать несколько несвязанных частей.
    /// </summary>
    public IReadOnlyList<Polygon2D> UnionedFloorOutline { get; }

    /// <summary>
    /// Z-координата (в футах) плиты, по которой строятся detail-линии.
    /// </summary>
    public double SlabElevation { get; }
}
