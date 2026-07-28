using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitPylonLoadAreas.Services;

namespace RevitPylonLoadAreas.Models.Geometry.Voronoi;

internal class FloorVoronoiData {
    private readonly RevitRepository _repo;

    /// <summary>
    /// Пороговая площадь, меньше которой отверстия не учитываются
    /// </summary>
    private readonly double _openingsAreaThreshold;

    /// <summary>
    /// Данные диаграммы Вороного для каждой нижней грани солида перекрытия
    /// </summary>
    private IList<FaceVoronoiData> _facesData;

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
    }

    public Floor Floor { get; }

    public bool IsInside(XY point) {
        return GetFacesData().Any(f => f.IsInside(point));
    }

    public Solid GetVoronoiSolid() {
        var faces = GetFacesData();
        var solid = faces.First().GetVoronoiSolid();
        for(int i = 1; i < faces.Count; i++) {
            solid = _repo.Unite(solid, faces[i].GetVoronoiSolid());
        }

        return solid;
    }

    private IList<FaceVoronoiData> GetFacesData() {
        if(_facesData is not null) {
            return _facesData;
        }

        var solids = Floor.GetSolids();
        _facesData = solids.SelectMany(s => _repo.GetBottomFaces(s))
            .Select(f => new FaceVoronoiData(f, _repo, _openingsAreaThreshold))
            .ToArray();
        return _facesData;
    }
}
