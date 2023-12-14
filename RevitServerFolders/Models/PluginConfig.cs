using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitServerFolders.Models {
    internal abstract class PluginConfig : ProjectConfig {
        [JsonIgnore] public override string ProjectConfigPath { get; set; }

        [JsonIgnore] public override IConfigSerializer Serializer { get; set; }
        
        public string TargetFolder { get; set; }
        public string SourceFolder { get; set; }
        public string[] SkippedObjects { get; set; }
    }

    internal class FileModelObjectConfig : PluginConfig {
        public bool IsExportRooms { get; set; }
        
        public static FileModelObjectConfig GetPluginConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitServerFolders))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(FileModelObjectConfig) + ".json")
                .Build<FileModelObjectConfig>();
        }
    }
    
    internal class RsModelObjectConfig : PluginConfig {
        public static RsModelObjectConfig GetPluginConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitServerFolders))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(RsModelObjectConfig) + ".json")
                .Build<RsModelObjectConfig>();
        }
    }
}
