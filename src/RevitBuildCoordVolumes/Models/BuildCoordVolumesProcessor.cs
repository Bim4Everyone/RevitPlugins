using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models;
internal class BuildCoordVolumesProcessor {
    private readonly ILocalizationService _localizationService;
    private readonly IExtrusionBuilder _builder;
    private readonly RevitRepository _revitRepository;
    private readonly BuildCoordVolumesSettings _settings;
    private readonly BuilderFactory _builderFactory;

    public BuildCoordVolumesProcessor(
        ILocalizationService localizationService,
        RevitRepository revitRepository,
        BuildCoordVolumesSettings settings) {
        _localizationService = localizationService;
        _revitRepository = revitRepository;
        _settings = settings;
        _builderFactory = new BuilderFactory(_revitRepository, _settings);
        _builder = _builderFactory.Create(_settings.AlgorithmType);
    }

    public List<RevitArea> RevitAreas => GetRevitAreas();


    public void Run(IProgress<int> progress = null, CancellationToken ct = default) {

        string transactionName = _localizationService.GetLocalizedString("BuildCoordVolumesProcessor.TransactionName");
        using var t = _revitRepository.Document.StartTransaction(transactionName);

        int pro = 0;
        foreach(var revitArea in RevitAreas) {
            ct.ThrowIfCancellationRequested();
            var geomElements = _builder.BuildVolumes(revitArea);
            progress?.Report(++pro);
        }
        t.Commit();
    }

    private List<RevitArea> GetRevitAreas() {
        string areaType = _settings.TypeZone;
        var areaTypeParam = _settings.ParamMaps.First().SourceParam;
        return _revitRepository.GetRevitAreas(areaType, areaTypeParam).ToList();
    }
}
