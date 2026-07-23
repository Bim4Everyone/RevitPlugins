using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonLoadAreas.Services;

namespace RevitPylonLoadAreas.Models.Geometry.Voronoi;

internal class FaceVoronoiData {
    private readonly RevitRepository _repo;
    private readonly CurveLoopsSimplifier _simplifier;

    /// <summary>
    /// Пороговая площадь, меньше которой отверстия не учитываются
    /// </summary>
    private readonly double _openingsAreaThreshold;

    /// <summary>
    /// Исходная нижняя грань перекрытия
    /// </summary>
    private readonly PlanarFace _sourceFace;

    /// <summary>
    /// Контур грани в плоскости XOY для построения грузовых площадей с учетом заданной пороговой площади отверстий
    /// </summary>
    private IList<CurveLoop> _voronoiOutline;

    /// <summary>
    /// Грань для построения диаграммы Вороного с учетом заданной пороговой площади отверстий
    /// </summary>
    private PlanarFace _voronoiFace;

    /// <summary>
    /// Поверхность <see cref="_voronoiFace"/>
    /// </summary>
    private Surface _voronoiSurface;

    /// <summary>
    /// Солид грани с учетом заданной пороговой площади отверстий
    /// </summary>
    private Solid _voronoiSolid;

    /// <summary>
    /// Конструирует нижнюю грань перекрытия для построения на ней диаграммы Вороного
    /// </summary>
    /// <param name="face">Нижняя грань перекрытия</param>
    /// <param name="repo">Revit репозиторий</param>
    /// <param name="openingsAreaThreshold">Площадь отверстий в единицах Revit, меньше которой они не учитываются</param>
    public FaceVoronoiData(PlanarFace face, RevitRepository repo, double openingsAreaThreshold) {
        _sourceFace = face ?? throw new ArgumentNullException(nameof(face));
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _openingsAreaThreshold = openingsAreaThreshold;
        _simplifier = new CurveLoopsSimplifier();
    }

    public bool IsInside(XY point) {
        var face = GetVoronoiFace();
        _voronoiSurface ??= face.GetSurface();
        _voronoiSurface.Project(point.AsXYZ(), out var uv, out _);
        return face.IsInside(uv);
    }

    /// <summary>
    /// Возвращает солид грани в плоскости XOY с учетом заданной пороговой площади отверстий,
    /// по которому производится обрезка ячеек диаграммы Вороного
    /// </summary>
    public Solid GetVoronoiSolid() {
        return _voronoiSolid ??= _repo.CreateSolid(1, [..GetVoronoiOutline()]);
    }

    /// <summary>
    /// Возвращает список контуров грани в плоскости XOY с учетом заданной пороговой площади отверстий
    /// </summary>
    /// <returns>Первая петля - наружняя, остальные - отверстия, удовлетворяющие допуску</returns>
    private IList<CurveLoop> GetVoronoiOutline() {
        if(_voronoiOutline is not null) {
            return _voronoiOutline;
        }

        double z = _sourceFace.Evaluate(new UV(0, 0)).Z;
        var transform = Transform.CreateTranslation(new XYZ(0, 0, -z));
        _voronoiOutline = _simplifier.GetEdgesAsSimplifiedCurveLoops(_sourceFace)
            .Where(l => _repo.GetArea(l) >= _openingsAreaThreshold)
            .Select(l => CurveLoop.CreateViaTransform(l, transform))
            .OrderBy(l => l.Sum(c => c.Length))
            .ToArray();
        return _voronoiOutline;
    }

    private PlanarFace GetVoronoiFace() {
        return _voronoiFace ??= _repo.GetBottomFace(GetVoronoiSolid());
    }
}
