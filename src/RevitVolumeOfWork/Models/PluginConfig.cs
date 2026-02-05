using System.Collections.Generic;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitVolumeOfWork.Models; 
internal class PluginConfig : ProjectConfig<RevitSettings> {
    [JsonIgnore] 
    public override string ProjectConfigPath { get; set; }

    [JsonIgnore] 
    public override IConfigSerializer Serializer { get; set; }

    public static PluginConfig GetPluginConfig() {
        return new ProjectConfigBuilder()
            .SetSerializer(new ConfigSerializer())
            .SetPluginName(nameof(RevitVolumeOfWork))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(PluginConfig) + ".json")
            .Build<PluginConfig>();
    }
}

internal class RevitSettings : ProjectSettings {
    public override string ProjectName { get; set; }
    public IList<string> Levels { get; set; } = [];
}
