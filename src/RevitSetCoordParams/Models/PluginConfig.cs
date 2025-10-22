using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

using RevitSetCoordParams.Models.Settings;

namespace RevitSetCoordParams.Models;

internal class PluginConfig : ProjectConfig<RevitSettings> {

    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }

    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }

    public static PluginConfig GetPluginConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitSetCoordParams))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(PluginConfig) + ".json")
            .Build<PluginConfig>();
    }
}

internal class RevitSettings : ProjectSettings {
    public override string ProjectName { get; set; }
    public ConfigSettings ConfigSettings { get; set; }
}
