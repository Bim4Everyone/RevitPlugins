using RevitLintelsManager.Models.Settings;

namespace RevitLintelsManager.Models.Configs;

internal class PluginConfigRepository {
    private readonly PluginConfig _pluginConfig;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly RevitRepository _revitRepository;
    
    public PluginConfigRepository(
        PluginConfig pluginConfig, 
        SystemPluginConfig systemPluginConfig, 
        RevitRepository revitRepository) {
        _pluginConfig = pluginConfig;
        _systemPluginConfig = systemPluginConfig;
        _revitRepository = revitRepository;
    }
    
    
    public LintelManagerSettings LoadSettings() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document);
        return setting?.LintelManagerSettings;
    }
    
    public void SaveSettings(LintelManagerSettings lintelManagerSettings) {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                      ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.LintelManagerSettings = lintelManagerSettings;
        _pluginConfig.SaveProjectConfig();
    }
    
    public LintelManagerSettings GetDefaultLintelManagerSettings() {
        return new LintelManagerSettings {
            LintelFamilySettings = GetLintelFamilySettings(),
            OpeningFamilySettings = GetOpeningFamilySettings(),
            LintelConfigRuleName = _systemPluginConfig.DefaultLintelConfigRuleName,
            LintelFixParamName = _systemPluginConfig.DefaultLintelFixParamName,
            MinimalHeightAboveOpeningMm = _systemPluginConfig.DefaultMinimalHeightAboveLintelMm,
            StructureWallTypeNames = _systemPluginConfig.DefaultStructureWallTypeNames,
            PhaseName = _systemPluginConfig.DefaultPhaseName,
        };
    }

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

    private OpeningFamilySettings GetOpeningFamilySettings() {
        return new OpeningFamilySettings {
            FamilyNames = _systemPluginConfig.DefaultOpeningFamilyNames,
            LintelOpeningHeight = _systemPluginConfig.OpeningHeightParamName,
            LintelOpeningWidth = _systemPluginConfig.OpeningWidthParamName
        };
    }
}
