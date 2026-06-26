using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitPylonLoadAreas.Services;

namespace RevitPylonLoadAreas.Models.Geometry.Voronoi;

internal class FloorVoronoiData {
    private readonly RevitRepository _repo;
    private readonly CurveLoopsSimplifier _simplifier;

    /// <summary>
    /// Пороговая площадь, меньше которой отверстия не учитываются
    /// </summary>
    private readonly double _openingsAreaThreshold;

    /// <summary>
    /// Контур перекрытия в плоскости XOY для построения грузовых площадей с учетом заданной пороговой площади отверстий
    /// </summary>
    private IList<CurveLoop> _outline;

    /// <summary>
    /// Грань перекрытия для построения грузовых площадей с учетом заданной пороговой площади отверстий
    /// </summary>
    private PlanarFace _face;

    /// <summary>
    /// Поверхность <see cref="_face"/>
    /// </summary>
    private Surface _surface;

    /// <summary>
    /// Солид перекрытия с учетом заданной пороговой площади отверстий
    /// </summary>
    private Solid _solid;

    /// <summary>
    /// Конструирует перекрытие для построения на нём диаграммы Вороного
    /// </summary>
    /// <param name="floor">Перекрытие</param>
    /// <param name="repo">Revit репозиторий</param>
    /// <param name="openingsAreaThreshold">Площадь отверстий в единицах Revit, меньше которой они не учитываются</param>
    public FloorVoronoiData(Floor floor, RevitRepository repo, double openingsAreaThreshold) {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _openingsAreaThreshold = openingsAreaThreshold;
        Floor = floor ?? throw new ArgumentNullException(nameof(floor));
        _simplifier = new CurveLoopsSimplifier();
    }

    public Floor Floor { get; }

    public bool IsInside(XY point) {
        var face = GetVoronoiFace();
        _surface ??= face.GetSurface();
        _surface.Project(point.AsXYZ(), out var uv, out _);
        return face.IsInside(uv);
    }

    /// <summary>
    /// Обрезает ячейку диаграммы Вороного по контуру перекрытием с учетом заданной пороговой площади отверстий
    /// </summary>
    /// <param name="cell">Ячейка диаграммы Вороного</param>
    /// <returns>Список петель в плоскости XOY плоской фигуры, полученной после обрезки</returns>
    public IList<CurveLoop> Clip(VoronoiCell cell) {
        var cellSolid = _repo.CreateSolid(1, cell.Polygon.AsCurveLoop());
        var floorSolid = GetVoronoiSolid();
        var intersection = _repo.Intersect(cellSolid, floorSolid);
        var bottomFace = _repo.GetBottomFace(intersection);
        return _simplifier.GetEdgesAsSimplifiedCurveLoops(bottomFace);
    }

    /// <summary>
    /// Объединяет заданные ячейки и обрезает их по контуру перекрытия с учетом заданной пороговой площади отверстий
    /// </summary>
    /// <param name="wallCells">Ячейки диаграммы Вороного, построенные для одной стены</param>
    /// <returns>Список петель ячейки диаграммы для всей стены в плоскости XOY</returns>
    public IList<CurveLoop> Clip(IList<VoronoiCell> wallCells) {
        var unitedSolid = CreateUnitedSolid(wallCells);
        var intersection = _repo.Intersect(unitedSolid, GetVoronoiSolid());
        var bottomFaces = _repo.GetBottomFaces(intersection);
        return bottomFaces.SelectMany(f => _simplifier.GetEdgesAsSimplifiedCurveLoops(f)).ToArray();
    }

    private Solid CreateUnitedSolid(IList<VoronoiCell> cells) {
        var solid = _repo.CreateSolid(1, cells[0].Polygon.AsCurveLoop());
        for(int i = 1; i < cells.Count; i++) {
            var isolid = _repo.CreateSolid(1, cells[i].Polygon.AsCurveLoop());
            solid = _repo.Unite(solid, isolid);
        }

        return solid;
    }

    /// <summary>
    /// Возвращает список контуров перекрытия в плоскости XOY с учетом заданной пороговой площади отверстий
    /// </summary>
    /// <returns>Первая петля - наружняя, остальные - отверстия, удовлетворяющие допуску</returns>
    private IList<CurveLoop> GetVoronoiOutline() {
        if(_outline is not null) {
            return _outline;
        }

        // у 1 элемента перекрытия должно быть только 1 тело
        var solid = Floor.GetSolids()
            .OrderByDescending(s => s.Volume)
            .First();
        var bottomFace = _repo.GetBottomFace(solid);
        double z = bottomFace.Evaluate(new UV(0, 0)).Z;
        var transform = Transform.CreateTranslation(new XYZ(0, 0, -z));
        _outline = _simplifier.GetEdgesAsSimplifiedCurveLoops(bottomFace)
            .Where(l => _repo.GetArea(l) >= _openingsAreaThreshold)
            .Select(l => CurveLoop.CreateViaTransform(l, transform))
            .OrderBy(l => l.Sum(c => c.Length))
            .ToArray();
        return _outline;
    }

    private PlanarFace GetVoronoiFace() {
        return _face ??= _repo.GetBottomFace(GetVoronoiSolid());
    }

    private Solid GetVoronoiSolid() {
        return _solid ??= _repo.CreateSolid(1, [..GetVoronoiOutline()]);
    }
}
