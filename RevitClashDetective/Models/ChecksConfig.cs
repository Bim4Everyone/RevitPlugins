using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.Models {
    internal class ChecksConfig : ProjectConfig {
        public string RevitVersion { get; set; }
        public List<Check> Checks { get; set; } = new List<Check>();
        [JsonIgnore]
        public override string ProjectConfigPath { get; set; }
        [JsonIgnore]
        public override IConfigSerializer Serializer { get; set; }

        public static ChecksConfig GetChecksConfig(string revitFileName) {
            return new ProjectConfigBuilder()
                .SetSerializer(new RevitClashConfigSerializer())
                .SetRelativePath(Path.Combine(nameof(RevitClashDetective), revitFileName))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(ChecksConfig) + ".json")
                .Build<ChecksConfig>();
        }
    }

    internal class Check {
        public string Name { get; set; }
        public SelectionConfig FirstSelection { get; set; }
        public SelectionConfig SecondSelection { get; set; }
    }

    internal class SelectionConfig {
        public List<string> Filters { get; set; } = new List<string>();
        public List<string> Files { get; set; } = new List<string>();
    }
}
