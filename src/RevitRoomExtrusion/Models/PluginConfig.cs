using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

namespace RevitRoomExtrusion.Models;

internal class PluginConfig : ProjectConfig<RevitSettings> {
    [JsonIgnore] public override string ProjectConfigPath { get; set; }
    [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

    public static PluginConfig GetPluginConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitRoomExtrusion))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(PluginConfig) + ".json")
            .Build<PluginConfig>();
    }
}

internal class RevitSettings : ProjectSettings {
    public override string ProjectName { get; set; }
    public string ExtrusionHeightMm { get; set; }
    public string ExtrusionFamilyName { get; set; }
    public bool IsJoinExtrusionChecked { get; set; }
}
