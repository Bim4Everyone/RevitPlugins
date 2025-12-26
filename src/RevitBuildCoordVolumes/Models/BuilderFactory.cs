using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models;
internal sealed class BuilderFactory {
    private readonly RevitRepository _revitRepository;
    private readonly BuildCoordVolumesServices _services;
    private readonly BuildCoordVolumesSettings _settings;

    public BuilderFactory(
        RevitRepository revitRepository,
        BuildCoordVolumesSettings settings) {
        _revitRepository = revitRepository;
        _services = new BuildCoordVolumesServices();
        _settings = settings;
    }

    public IExtrusionBuilder Create(AlgorithmType algorithmType) {
        return algorithmType == AlgorithmType.AdvancedAreaExtrude
            ? new ExtrusionContourBuilder(
                _services.SpatialDivider,
                _services.SlabNormalizer,
                _services.ColumnFactory,
                _services.GeomElementFactory,
                _revitRepository,
                _settings)
            : algorithmType == AlgorithmType.SimpleAreaExtrude
            ? new ExtrusionSimpleBuilder()
            : new ExtrusionSimpleBuilder();
    }
}
