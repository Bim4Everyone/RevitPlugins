using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitBuildCoordVolumes.Models.Geometry;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models;
internal class BuildCoordVolumesProcessor {
    private readonly ILocalizationService _localizationService;
    private readonly IExtrusionBuilder _builder;
    private readonly RevitRepository _revitRepository;
    private readonly BuildCoordVolumesSettings _settings;
    private readonly BuildCoordVolumesServices _services;
    private readonly BuilderFactory _builderFactory;

    public BuildCoordVolumesProcessor(
        ILocalizationService localizationService,
        RevitRepository revitRepository,
        BuildCoordVolumesSettings settings,
        BuildCoordVolumesServices services) {
        _localizationService = localizationService;
        _revitRepository = revitRepository;
        _settings = settings;
        _services = services;
        _builderFactory = new BuilderFactory(_revitRepository, _settings, _services);
        _builder = _builderFactory.Create(_settings.AlgorithmType);
    }

    public List<SpatialObject> SpatialObjects => GetSpatialObjects();

    public void Run(IProgress<int> progress = null, CancellationToken ct = default) {

        string transactionName = _localizationService.GetLocalizedString("BuildCoordVolumesProcessor.TransactionName");
        using var t = _revitRepository.Document.StartTransaction(transactionName);

        int pro = 0;
        foreach(var spatialObject in SpatialObjects) {
            ct.ThrowIfCancellationRequested();

            var geomElements = _builder.BuildVolumes(spatialObject);

            var directShapeElements = _services.DirectShapeObjectFactory.GetDirectShapeObjects(geomElements, _revitRepository);

            _services.ParamSetter.SetParams(spatialObject.SpatialElement, directShapeElements, _settings);

            progress?.Report(++pro);
        }
        t.Commit();
    }

    private List<SpatialObject> GetSpatialObjects() {
        string areaType = _settings.TypeZone;
        var areaTypeParam = _settings.ParamMaps.First().SourceParam;
        return _revitRepository.GetSpatialObjects(areaType, areaTypeParam).ToList();
    }
}
