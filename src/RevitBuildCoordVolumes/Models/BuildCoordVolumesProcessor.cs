using dosymep.Revit;

using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models;

internal class BuildCoordVolumesProcessor {
    private readonly RevitRepository _revitRepository;
    private readonly BuildCoordVolumeSettings _settings;
    private readonly BuildCoordVolumeServices _services;
    private readonly ICoordVolumeBuilder _builder;
    private readonly BuilderFactory _builderFactory;

    public BuildCoordVolumesProcessor(
        RevitRepository revitRepository,
        BuildCoordVolumeSettings settings,
        BuildCoordVolumeServices services) {
        _revitRepository = revitRepository;
        _settings = settings;
        _services = services;
        _builderFactory = new BuilderFactory(_revitRepository, _settings, _services);
        _builder = _builderFactory.Create(_settings.AlgorithmType);
    }
    /// <summary>
    /// Основной метод построения и обработки объемных элементов.
    /// </summary>
    /// <remarks>
    /// В данном методе происходит транзакция, экструзия объемных элементов и назначения им параметров по исходным зонам.
    /// </remarks>
    /// <param name="progressService">Прогресс сервис.</param>
    /// <returns>
    /// Void.
    /// </returns>
    public void Run(ProgressService progressService = null) {
        string transactionName = _services.LocalizationService.GetLocalizedString("BuildCoordVolumesProcessor.TransactionName");
        using var t = _revitRepository.Document.StartTransaction(transactionName);
        var spatialObjects = _settings.SpatialObjects;

        for(int i = 0; i < spatialObjects.Count; i++) {
            progressService?.CancellationToken.ThrowIfCancellationRequested();
            progressService.ZoneNumber = (i + 1).ToString();

            var geomElements = _builder.Build(spatialObjects[i], progressService);

            var directShapeElements = _services.DirectShapeObjectFactory.GetDirectShapeObjects(geomElements, _revitRepository);

            _services.ParamSetter.SetParams(spatialObjects[i].SpatialElement, directShapeElements, _settings);
        }
        t.Commit();
    }
}
