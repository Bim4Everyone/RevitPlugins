using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitCopyInteriorSpecs.Models;
internal class PluginConfig : ProjectConfig<RevitSettings> {
    [JsonIgnore] public override string ProjectConfigPath { get; set; }

    [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

    public static PluginConfig GetPluginConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitCopyInteriorSpecs))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(PluginConfig) + ".json")
            .Build<PluginConfig>();
    }
}

internal class RevitSettings : ProjectSettings {
    public override string ProjectName { get; set; }
    public string GroupTypeParamName { get; set; }
    public string LevelParamName { get; set; }
    public string LevelShortNameParamName { get; set; }
    public string PhaseParamName { get; set; }
    public string FirstDispatcherGroupingLevelParamName { get; set; }
    public string SecondDispatcherGroupingLevelParamName { get; set; }
    public string ThirdDispatcherGroupingLevelParamName { get; set; }
}
