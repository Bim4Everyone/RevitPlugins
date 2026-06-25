using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitPylonLoadAreas.Models;
using RevitPylonLoadAreas.Models.Geometry;
using RevitPylonLoadAreas.Services.Core;

namespace RevitPylonLoadAreas.Services;

internal class LandThicknessFinder {
    private readonly RevitRepository _repo;
    private readonly LandXmlImporter _landXmlImporter;
    private readonly LoadAreasFinder _loadAreasFinder;
    private readonly IOpenFileDialogService _openFileDialog;
    private readonly ILocalizationService _localization;
    private readonly IErrorsService _errorsService;
    private readonly LandXmlImportConfig _config;

    public LandThicknessFinder(
        RevitRepository repo,
        LandXmlImporter landXmlImporter,
        LoadAreasFinder loadAreasFinder,
        IOpenFileDialogService openFileDialog,
        ILocalizationService localization,
        IErrorsService errorsService,
        LandXmlImportConfig config) {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _landXmlImporter = landXmlImporter ?? throw new ArgumentNullException(nameof(landXmlImporter));
        _loadAreasFinder = loadAreasFinder ?? throw new ArgumentNullException(nameof(loadAreasFinder));
        _openFileDialog = openFileDialog ?? throw new ArgumentNullException(nameof(openFileDialog));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public void FindAndSetLandThickness(
        ICollection<LoadArea> pylonLoadAreas,
        ICollection<Polygon3D> landPolygons,
        IProgress<int> progress = null,
        CancellationToken ct = default) {
        int i = 0;
        foreach(var loadArea in pylonLoadAreas) {
            ct.ThrowIfCancellationRequested();
            progress?.Report(++i);
            Polygon3D[] intersectingLandPolygons;
            try {
                intersectingLandPolygons = landPolygons
                    .Where(p => loadArea.Intersects(p.AsPolygon2D()))
                    .ToArray();
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                _errorsService.AddError(loadArea.Element, "Error.CannotCalculateLandThickness");
                continue;
            }

            if(intersectingLandPolygons.Length == 0) {
                continue;
            }

            if(intersectingLandPolygons.Any(p => p.IntersectsXOY())) {
                // LandXml был выгружен из некорректной модели ГП - часть слоя земли имеет отрицательную толщину
                _errorsService.AddError(loadArea.Element, "Error.InvalidLandXmlData");
                continue;
            }

            double landVolume = 0;
            try {
                landVolume = GetLandVolume(loadArea, intersectingLandPolygons);
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                _errorsService.AddError(loadArea.Element, "Error.CannotCalculateLandThickness");
                continue;
            }

            double thickness = UnitUtils.ConvertFromInternalUnits(landVolume / loadArea.GetArea(), UnitTypeId.Meters);
            loadArea.Element.SetParamValue(_repo.LandThicknessParam, thickness);
        }
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
}
