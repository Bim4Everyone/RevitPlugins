using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models;
internal class BuildCoordVolumesProcessor {

    private readonly ILocalizationService _localizationService;
    private readonly RevitRepository _revitRepository;
    private readonly BuildCoordVolumesSettings _settings;
    private readonly SolidsService _solidsService;
    private readonly GeometryService _geometryService;

    public BuildCoordVolumesProcessor(
        ILocalizationService localizationService,
        RevitRepository revitRepository,
        BuildCoordVolumesSettings settings) {
        _localizationService = localizationService;
        _revitRepository = revitRepository;
        _settings = settings;
        _solidsService = new SolidsService();
        _geometryService = new GeometryService();
    }

    public IEnumerable<RevitArea> Areas => GetRevitAreas();

    public void Run(IProgress<int> progress = null, CancellationToken ct = default) {

        IBuildAreaExtrusion buildAreaExtrusion;
        buildAreaExtrusion = _settings.AlgorithmType == AlgorithmType.AdvancedAreaExtrude
            ? new AdvancedAreaExtrusionBuilder(_revitRepository, _settings, _solidsService, _geometryService)
            : new AreaExtrusionBuilder();

        string transactionName = _localizationService.GetLocalizedString("BuildCoordVolumesProcessor.TransactionName");
        using var t = _revitRepository.Document.StartTransaction(transactionName);
        int i = 0;
        foreach(var area in Areas) {
            ct.ThrowIfCancellationRequested();

            buildAreaExtrusion.BuildAreaExtrusion(area);

            progress?.Report(++i);
        }

        t.Commit();
    }

    // Метод получения элементов модели для основного метода и прогресс-бара  
    private IEnumerable<RevitArea> GetRevitAreas() {
        string areaType = _settings.TypeZone;
        var areaTypeParam = _settings.ParamMaps.First().SourceParam;
        return _revitRepository.GetRevitAreas(areaType, areaTypeParam);
    }
}



