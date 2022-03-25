using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitServerFolders.Export
{
    public class ExportNwcFileConfig : ProjectConfig {
        [JsonIgnore]
        public override string ProjectConfigPath { get; set; }
        
        [JsonIgnore]
        public override IConfigSerializer Serializer { get; set; }
        
        public bool WithRooms { get; set; }
        public bool WithLinkedFiles { get; set; }
        public bool WithSubFolders { get; set; }

        public string SourceNwcFolder { get; set; }
        public string TargetNwcFolder { get; set; }

        public bool CleanTargetNwcFolder { get; set; }
        
        public static ExportNwcFileConfig GetExportNwcFileConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitServerFolders))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(ExportNwcFileConfig) + ".json")
                .Build<ExportNwcFileConfig>();
        }

    }
}