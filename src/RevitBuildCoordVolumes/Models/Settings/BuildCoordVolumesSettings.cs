namespace RevitBuildCoordVolumes.Models.Settings;

internal class BuildCoordVolumesSettings {

    private readonly RevitRepository _revitRepository;
    private readonly ProvidersFactory _providersFactory;

    public BuildCoordVolumesSettings(RevitRepository revitRepository, ConfigSettings configSettings) {
        _revitRepository = revitRepository;
        ConfigSettings = configSettings;
        _providersFactory = new ProvidersFactory();
    }

    public ConfigSettings ConfigSettings { get; private set; }
}
