using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

namespace RevitRefreshLinks.Models;
internal class UpdateLinksConfig : ProjectConfig<UpdateLinksSettings> {
    [JsonIgnore] public override string ProjectConfigPath { get; set; }

    [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

    public static UpdateLinksConfig GetPluginConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitRefreshLinks))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(UpdateLinksConfig) + ".json")
            .Build<UpdateLinksConfig>();
    }
}

internal class UpdateLinksSettings : ProjectSettings {
    public override string ProjectName { get; set; }
    public string InitialFolderPath { get; set; }
    public string InitialServerPath { get; set; }
}
