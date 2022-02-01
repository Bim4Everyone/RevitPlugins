using System;
using System.IO;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitServerFolders.Export {
    public class ExportRvtFileConfig : ProjectConfig {
        public override string ProjectConfigPath { get; set; }
        public override IConfigSerializer Serializer { get; set; }

        public string ServerName { get; set; }

        public bool WithRooms { get; set; }
        public bool WithNwcFiles { get; set; }
        public bool WithSubFolders { get; set; }

        public string SourceRvtFolder { get; set; }
        public string TargetRvtFolder { get; set; }
        public string TargetNwcFolder { get; set; }

        public bool CleanTargetRvtFolder { get; set; }
        public bool CleanTargetNwcFolder { get; set; }

        public static ExportRvtFileConfig GetExportRvtFileConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitServerFolders))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(ExportRvtFileConfig) + ".json")
                .Build<ExportRvtFileConfig>();
        }
    }
}