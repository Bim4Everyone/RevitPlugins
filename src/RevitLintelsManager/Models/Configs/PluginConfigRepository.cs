using RevitLintelsManager.Models.Settings;

namespace RevitLintelsManager.Models.Configs;

internal class PluginConfigRepository : IPluginConfigRepository {
    private readonly PluginConfig _pluginConfig;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly RevitRepository _revitRepository;

    /// <summary>Создаёт PluginConfigRepository с переданными зависимостями.</summary>
    public PluginConfigRepository(
        PluginConfig pluginConfig, 
        SystemPluginConfig systemPluginConfig, 
        RevitRepository revitRepository) {
        _pluginConfig = pluginConfig;
        _systemPluginConfig = systemPluginConfig;
        _revitRepository = revitRepository;
    }
    
    
    public string ConfigPath => _pluginConfig.ProjectConfigPath;

    /// <inheritdoc />
    public LintelManagerSettings LoadSettings() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document);
        return setting?.LintelManagerSettings;
    }
    
    /// <inheritdoc />
    public void SaveSettings(LintelManagerSettings lintelManagerSettings) {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                      ?? _pluginConfig.AddSettings(_revitRepository.Document);

        var previousSettings = setting.LintelManagerSettings;
        setting.LintelManagerSettings = lintelManagerSettings;
        try {
            _pluginConfig.SaveProjectConfig();
        } catch {
            setting.LintelManagerSettings = previousSettings;
            throw;
        }
    }
    
    /// <inheritdoc />
    public LintelManagerSettings GetDefaultLintelManagerSettings() {
        return new LintelManagerSettings {
            LintelFamilySettings = GetLintelFamilySettings(),
            OpeningFamilySettings = GetOpeningFamilySettings(),
            LintelConfigRuleName = _systemPluginConfig.DefaultLintelConfigRuleName,
            LintelFixParamName = _systemPluginConfig.DefaultLintelFixParamName,
            MinimalHeightAboveOpeningMm = _systemPluginConfig.DefaultMinimalHeightAboveLintelMm,
            MinimalBearingMm = _systemPluginConfig.DefaultMinimalBearingMm,
            StructureWallTypeNames = [],
            ReinforcedConcreteTypes = [],
            SteelTypes = [],
            PhaseName = _systemPluginConfig.DefaultPhaseName,
        };
    }

    // Создаёт настройки семейства перемычки по умолчанию.
    private LintelFamilySettings GetLintelFamilySettings() {
        return  new LintelFamilySettings {
            LintelFamily = _systemPluginConfig.DefaultLintelFamilyName,
            LintelWidth = _systemPluginConfig.LintelWidthParamName,
            LintelThickness = _systemPluginConfig.LintelThicknessParamName,
            LintelRightOffset = _systemPluginConfig.LintelRightOffsetParamName,
            LintelLeftOffset = _systemPluginConfig.LintelLeftOffsetParamName,
            LintelRightCorner = _systemPluginConfig.LintelRightCornerParamName,
            LintelLeftCorner = _systemPluginConfig.LintelLeftCornerParamName,
            LintelRightWelding = _systemPluginConfig.LintelRightWeldingParamName,
            LintelLeftWelding = _systemPluginConfig.LintelLeftWeldingParamName
        };
    }

    // Создаёт настройки семейств проёмов по умолчанию.
    private OpeningFamilySettings GetOpeningFamilySettings() {
        return new OpeningFamilySettings {
            FamilyNames = _systemPluginConfig.DefaultOpeningFamilyNames,
            LintelOpeningHeight = _systemPluginConfig.OpeningHeightParamName,
            LintelOpeningWidth = _systemPluginConfig.OpeningWidthParamName
        };
    }
}
