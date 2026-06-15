using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using Ninject;

using RevitPylonLoadAreas.Models;
using RevitPylonLoadAreas.Models.Geometry;

namespace RevitPylonLoadAreas.Services;

internal class LandThicknessFinder {
    private readonly RevitRepository _repo;
    private readonly LandXmlImporter _landXmlImporter;
    private readonly LoadAreasFinder _loadAreasFinder;
    private readonly IOpenFileDialogService _openFileDialogService;
    private readonly ILocalizationService _localization;

    public LandThicknessFinder(
        RevitRepository repo,
        LandXmlImporter landXmlImporter,
        LoadAreasFinder loadAreasFinder,
        IOpenFileDialogService openFileDialog,
        ILocalizationService localization) {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _landXmlImporter = landXmlImporter ?? throw new ArgumentNullException(nameof(landXmlImporter));
        _loadAreasFinder = loadAreasFinder ?? throw new ArgumentNullException(nameof(loadAreasFinder));
        _openFileDialogService =
            openFileDialog ?? throw new ArgumentNullException(nameof(openFileDialog));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
    }

    public void FindAndSetLandThickness() {
        var loadAreas = GetPylonLoadAreas();
        var landPolygons = GetLandPolygons();
        foreach(var loadArea in loadAreas) {
            var intersectingLandPolygons = landPolygons
                .Where(p => p.Get2DVertices().Any(v => loadArea.IsInside(v)))
                .ToArray();
            if(intersectingLandPolygons.Length == 0) {
                continue;
            }

            if(intersectingLandPolygons.Any(p => p.IntersectsXOY())) {
                // LandXml был выгружен из некорректной модели ГП - часть слоя земли имеет отрицательную толщину
                // TODO добавить соответствующую ошибку в ErrorsService
                continue;
            }

            double landVolume = 0;
            try {
                landVolume = GetLandVolume(loadArea, intersectingLandPolygons);
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                // TODO добавить соответствующую ошибку в ErrorsService
                continue;
            }

            double thickness = UnitUtils.ConvertFromInternalUnits(landVolume / loadArea.GetArea(), UnitTypeId.Meters);
            loadArea.Element.SetParamValue(RevitRepository.LandThicknessParamName, thickness);
        }
    }

    private ICollection<Polygon3D> GetLandPolygons() {
        if(!_openFileDialogService.ShowDialog()) {
            throw new OperationCanceledException();
        }

        // в LandXml файле должна быть выгружена разность проектной поверхности ГП и поверхности плиты,
        // при такой выгрузке в LandXml по координате Z лежат готовые толщины слоя земли над плитой
        return _landXmlImporter.Import(_openFileDialogService.File.FullName);
    }

    private double GetLandVolume(LoadArea loadArea, ICollection<Polygon3D> intersectingLandPolygons) {
        double volume = 0;
        var loadAreaSolid = _repo.CreateSolid(intersectingLandPolygons.Max(p => p.GetMaxZ()), [..loadArea.Circuits]);
        foreach(var landPolygon in intersectingLandPolygons) {
            var landSolid = _repo.CreateSolid(landPolygon);
            var intersection = _repo.Intersect(loadAreaSolid, landSolid);
            volume += intersection.Volume;
        }

        return volume;
    }

    private ICollection<LoadArea> GetPylonLoadAreas() {
        var floors = _repo.PickFloors(_localization.GetLocalizedString("Pick.Floors"));
        var pylons = _repo.GetPylonsFromView();
        var walls = _repo.GetWallsFromView();

        List<LoadArea> loadAreas = [];
        foreach(var floor in floors) {
            loadAreas.AddRange(_loadAreasFinder.Process(floor, pylons, walls).Where(l => l.ElementIsPylon()));
        }

        return loadAreas;
    }
}
