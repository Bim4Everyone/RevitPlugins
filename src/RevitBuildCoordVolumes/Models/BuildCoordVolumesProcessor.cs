using System;
using System.Threading;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models;

internal class BuildCoordVolumesProcessor {
    private readonly RevitRepository _revitRepository;
    private readonly BuildCoordVolumeSettings _settings;
    private readonly BuildCoordVolumeServices _services;
    private readonly ILocalizationService _localizationService;
    private readonly ICoordVolumeBuilder _builder;
    private readonly BuilderFactory _builderFactory;

    public BuildCoordVolumesProcessor(
        RevitRepository revitRepository,
        BuildCoordVolumeSettings settings,
        BuildCoordVolumeServices services) {
        _revitRepository = revitRepository;
        _settings = settings;
        _services = services;
        _localizationService = _services.LocalizationService;
        _builderFactory = new BuilderFactory(_revitRepository, _settings, _services);
        _builder = _builderFactory.Create(_settings.AlgorithmType);
    }
    /// <summary>
    /// Основной метод построения и обработки объемных элементов.
    /// </summary>
    /// <remarks>
    /// В данном методе происходит транзакция, экструзия объемных элементов и назначения им параметров по исходным зонам.
    /// </remarks>
    /// <param name="progress">Прогресс.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>
    /// Void.
    /// </returns>
    public void Run(IProgress<int> progress = null, CancellationToken ct = default) {

        string transactionName = _localizationService.GetLocalizedString("BuildCoordVolumesProcessor.TransactionName");
        using var t = _revitRepository.Document.StartTransaction(transactionName);

        int pro = 0;
        foreach(var spatialObject in _settings.SpatialObjects) {
            ct.ThrowIfCancellationRequested();

            var geomElements = _builder.Build(spatialObject);

            var directShapeElements = _services.DirectShapeObjectFactory.GetDirectShapeObjects(geomElements, _revitRepository);

            _services.ParamSetter.SetParams(spatialObject.SpatialElement, directShapeElements, _settings);

            progress?.Report(++pro);
        }
        t.Commit();
    }
}
