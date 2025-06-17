using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

using RevitClashDetective.Models.FilterModel;

namespace RevitSleeves.Models.Config;

internal class PluginConfig : ProjectConfig {
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }

    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }

    public Set MepFilterSet { get; set; }

    public Offset[] Offsets { get; set; }

    public DiameterRange[] DiameterRanges { get; set; }

    public StructureSettings WallSettings { get; set; }

    public StructureSettings FloorSettings { get; set; }

    public bool ShowPlacingErrors { get; set; }

    public string Name { get; set; }

    public static PluginConfig GetPluginConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitSleeves))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(PluginConfig) + ".json")
            .Build<PluginConfig>();
    }
}
