using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitPylonLoadAreas.Models.Geometry;

/// <summary>
/// Грузовая площадь заданного вертикального несущего элемента
/// </summary>
internal sealed class LoadArea {
    private readonly RevitRepository _repo;
    private PlanarFace _face;
    private Solid _solid;

    /// <summary>
    /// Конструирует грузовую площадь
    /// </summary>
    /// <param name="element">Вертикальный элемент</param>
    /// <param name="repo">Репозиторий Revit</param>
    /// <param name="circuits">Список контуров грузовой площади в плоскости XOY. Первый контур - наружный, остальные - отверстия</param>
    public LoadArea(Element element, RevitRepository repo, IList<CurveLoop> circuits) {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        Element = element ?? throw new ArgumentNullException(nameof(element));
        Circuits = circuits ?? throw new ArgumentNullException(nameof(circuits));
        if(Circuits.Count == 0) {
            throw new ArgumentOutOfRangeException(nameof(circuits));
        }
    }

    public Element Element { get; }

    public IList<CurveLoop> Circuits { get; }

    public bool ElementIsPylon() {
        return Element is FamilyInstance pylon
               && pylon.Category.GetBuiltInCategory() == BuiltInCategory.OST_StructuralColumns;
    }

    /// <summary>
    /// Возвращает площадь в единицах Revit
    /// </summary>
    public double GetArea() {
        return GetFace().Area;
    }

    public bool Intersects(Polygon2D polygon) {
        var polygonSolid = _repo.CreateSolid(1, polygon.AsCurveLoop());
        var solid = GetSolid();
        return BooleanOperationsUtils.ExecuteBooleanOperation(polygonSolid, solid, BooleanOperationsType.Intersect)
                   .Volume
               > 0;
    }

    private PlanarFace GetFace() {
        return _face ??= _repo.GetTopFace(GetSolid());
    }

    private Solid GetSolid() {
        return _solid ??= _repo.CreateSolid(1, [..Circuits]);
    }
}
