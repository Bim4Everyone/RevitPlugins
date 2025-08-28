using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitRefreshLinks.Models;
internal class AddLinksFromFolderConfig : ProjectConfig<AddLinksFromFolderSettings> {
    [JsonIgnore] public override string ProjectConfigPath { get; set; }

    [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

    public static AddLinksFromFolderConfig GetPluginConfig() {
        return new ProjectConfigBuilder()
            .SetSerializer(new ConfigSerializer())
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
