using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models;
internal sealed class BuilderFactory {
    private readonly RevitRepository _revitRepository;
    private readonly BuildCoordVolumeServices _services;
    private readonly BuildCoordVolumeSettings _settings;

    public BuilderFactory(
        RevitRepository revitRepository,
        BuildCoordVolumeSettings settings,
        BuildCoordVolumeServices services) {
        _revitRepository = revitRepository;
        _settings = settings;
        _services = services;
    }

    public ICoordVolumeBuilder Create(AlgorithmType algorithmType) {
        return algorithmType == AlgorithmType.SlabBasedAlgorithm
            ? new SlabBasedCoordVolumeBuilder(
                _services.SpatialDivider,
                _services.SlabNormalizer,
                _services.ColumnFactory,
                _services.GeomObjectFactory,
                _revitRepository,
                _settings)
            : algorithmType == AlgorithmType.ParamBasedAlgorithm
            ? new ParamBasedCoordVolumeBuilder(
                _services.GeomObjectFactory,
                _revitRepository,
                _settings)
            : new ParamBasedCoordVolumeBuilder(
                _services.GeomObjectFactory,
                _revitRepository,
                _settings);
    }
}
