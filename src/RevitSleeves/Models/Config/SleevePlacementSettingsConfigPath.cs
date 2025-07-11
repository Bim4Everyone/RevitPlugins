using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

namespace RevitSleeves.Models.Config;
internal class SleevePlacementSettingsConfigPath : ProjectConfig {
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }

    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }

    /// <summary>
    /// Путь к файлу настроек расстановки гильз
    /// </summary>
    public string Path { get; set; }

    public static SleevePlacementSettingsConfigPath GetPluginConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitSleeves))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(SleevePlacementSettingsConfigPath) + ".json")
            .Build<SleevePlacementSettingsConfigPath>();
    }
}
