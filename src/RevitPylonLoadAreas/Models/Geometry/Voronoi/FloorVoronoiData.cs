using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

namespace RevitPylonLoadAreas.Models.Geometry.Voronoi;

internal class FloorVoronoiData {
    /// <summary>
    /// Пороговая площадь, меньше которой отверстия не учитываются
    /// </summary>
    private readonly double _openingsAreaThreshold;

    /// <summary>
    /// Контур перекрытий для построения грузовых площадей с учетом заданной пороговой площади отверстий
    /// </summary>
    private IList<CurveLoop> _outline;

    /// <summary>
    /// Грань перекрытия для построения грузовых площадей с учетом заданной пороговой площади отверстий
    /// </summary>
    private Face _face;

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
    /// <param name="openingsAreaThreshold">Площадь отверстий в единицах Revit, меньше которой они не учитываются</param>
    public FloorVoronoiData(Floor floor, double openingsAreaThreshold) {
        _openingsAreaThreshold = openingsAreaThreshold;
        Floor = floor ?? throw new ArgumentNullException(nameof(floor));
    }

    public Floor Floor { get; }

    public bool IsInside(XY point) {
        var face = GetVoronoiFace();
        _surface ??= face.GetSurface();
        _surface.Project(point.ToXYZ(), out var uv, out _);
        return face.IsInside(uv);
    }

    /// <summary>
    /// Обрезает ячейку диаграммы Вороного по контуру перекрытием с учетом заданной пороговой площади отверстий
    /// </summary>
    /// <param name="cell">Ячейка диаграммы Вороного</param>
    /// <returns>Список петель плоской фигуры, полученной после обрезки</returns>
    public IList<CurveLoop> Clip(VoronoiCell cell) {
        var cellSolid = CreateSolid(cell.Polygon.AsCurveLoop());
        var floorSolid = GetVoronoiSolid();
        var intersection = BooleanOperationsUtils.ExecuteBooleanOperation(
            cellSolid,
            floorSolid,
            BooleanOperationsType.Intersect);
        var topFace = GetTopFace(intersection);
        return topFace.GetEdgesAsCurveLoops();
    }

    /// <summary>
    /// Возвращает список контуров перекрытия в плоскости XOY с учетом заданной пороговой площади отверстий
    /// </summary>
    /// <returns>Первая петля - наружняя, остальные - отверстия, удовлетворяющие допуску</returns>
    public IList<CurveLoop> GetVoronoiOutline() {
        if(_outline is not null) {
            return _outline;
        }

        var solid = Floor.GetSolids()
            .OrderByDescending(s => s.Volume)
            .First();
        var topFace = GetTopFace(solid);
        _outline = topFace.GetEdgesAsCurveLoops()
            .Where(l => GetArea(l) >= _openingsAreaThreshold)
            .OrderBy(l => l.Sum(c => c.Length))
            .ToArray();
        return _outline;
    }

    private Face GetVoronoiFace() {
        return _face ??= GetTopFace(GetVoronoiSolid());
    }

    private Solid GetVoronoiSolid() {
        return _solid ??= CreateSolid([..GetVoronoiOutline()]);
    }

    private double GetArea(CurveLoop loop) {
        var solid = CreateSolid(loop);
        return GetTopFace(solid).Area;
    }

    private Face GetTopFace(Solid solid) {
        return solid.Faces
            .OfType<PlanarFace>()
            .First(f => f.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ));
    }

    private Solid CreateSolid(params CurveLoop[] loops) {
        if(loops.Length == 0) {
            throw new ArgumentOutOfRangeException(nameof(loops));
        }

        return GeometryCreationUtilities.CreateExtrusionGeometry(loops, XYZ.BasisZ, 1);
    }
}
