using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

namespace RevitServerFolders.Models;
internal abstract class PluginConfig : ProjectConfig {
    [JsonIgnore] public override string ProjectConfigPath { get; set; }

    [JsonIgnore] public override IConfigSerializer Serializer { get; set; }
}

internal abstract class PluginConfig<T> : PluginConfig where T : ExportSettings {
    public T[] ExportSettings { get; set; } = [];
}

internal class FileModelObjectConfig : PluginConfig<FileModelObjectExportSettings> {
    public static FileModelObjectConfig GetPluginConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitServerFolders))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(FileModelObjectConfig) + ".json")
            .Build<FileModelObjectConfig>();
    }
}

internal class RsModelObjectConfig : PluginConfig<RsModelObjectExportSettings> {
    public static RsModelObjectConfig GetPluginConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitServerFolders))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(RsModelObjectConfig) + ".json")
            .Build<RsModelObjectConfig>();
    }
}
