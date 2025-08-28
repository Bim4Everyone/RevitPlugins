using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitRefreshLinks.Models;
internal class AddLinksFromServerConfig : ProjectConfig<AddLinksFromServerSettings> {
    [JsonIgnore] public override string ProjectConfigPath { get; set; }

    [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

    public static AddLinksFromServerConfig GetPluginConfig() {
        return new ProjectConfigBuilder()
            .SetSerializer(new ConfigSerializer())
            .SetPluginName(nameof(RevitRefreshLinks))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(AddLinksFromServerConfig) + ".json")
            .Build<AddLinksFromServerConfig>();
    }
}

internal class AddLinksFromServerSettings : ProjectSettings {
    public override string ProjectName { get; set; }
    public string InitialServerPath { get; set; }
}
