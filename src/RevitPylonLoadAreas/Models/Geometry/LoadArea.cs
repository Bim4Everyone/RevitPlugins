using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitPylonLoadAreas.Models.Geometry;

/// <summary>
/// Грузовая площадь заданного вертикального несущего элемента
/// </summary>
internal sealed class LoadArea {
    private readonly RevitRepository _repo;
    private PlanarFace _face;
    private Surface _surface;

    /// <summary>
    /// Конструирует грузовую площадь
    /// </summary>
    /// <param name="element">Вертикальный элемент</param>
    /// <param name="repo">Репозиторий Revit</param>
    /// <param name="circuits">Список контуров грузовой площади. Первый контур - наружный, остальные - отверстия</param>
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

    public bool IsInside(XY point) {
        var face = GetFace();
        var surface = GetSurface();
        surface.Project(point.ToXYZ(), out var uv, out _);
        return face.IsInside(uv);
    }

    private PlanarFace GetFace() {
        return _face ??= _repo.GetTopFace(_repo.CreateSolid(1, [..Circuits]));
    }

    private Surface GetSurface() {
        return _surface ??= GetFace().GetSurface();
    }

    /// <summary>
    /// Возвращает площадь в единицах Revit
    /// </summary>
    public double GetArea() {
        return GetFace().Area;
    }
}
