using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

namespace RevitRefreshLinks.Models;
internal class AddLinksFromFolderConfig : ProjectConfig<AddLinksFromFolderSettings> {
    [JsonIgnore] public override string ProjectConfigPath { get; set; }

    [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

    public static AddLinksFromFolderConfig GetPluginConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitRefreshLinks))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(AddLinksFromFolderConfig) + ".json")
            .Build<AddLinksFromFolderConfig>();
    }
}

internal class AddLinksFromFolderSettings : ProjectSettings {
    public override string ProjectName { get; set; }
    public string InitialFolderPath { get; set; }
}
