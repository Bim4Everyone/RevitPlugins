using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Services;
using RevitBuildCoordVolumes.Models.Settings;

namespace RevitBuildCoordVolumes.Models;
internal sealed class BuilderFactory {
    private readonly RevitRepository _revitRepository;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly BuildCoordVolumesServices _services;
    private readonly BuildCoordVolumesSettings _settings;

    public BuilderFactory(
        RevitRepository revitRepository,
         SystemPluginConfig systemPluginConfig,
        BuildCoordVolumesSettings settings,
        BuildCoordVolumesServices services) {
        _revitRepository = revitRepository;
        _systemPluginConfig = systemPluginConfig;
        _settings = settings;
        _services = services;
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
            ? new ExtrusionSimpleBuilder(_services.GeomElementFactory, _revitRepository, _systemPluginConfig)
            : new ExtrusionSimpleBuilder(_services.GeomElementFactory, _revitRepository, _systemPluginConfig);
    }
}
