using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.Models {
    internal class ChecksConfig : ProjectConfig<CheckSettings> {
        [JsonIgnore]
        public override string ProjectConfigPath { get; set; }
        [JsonIgnore]
        public override IConfigSerializer Serializer { get; set; }

        public static ChecksConfig GetFiltersConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new RevitClashConfigSerializer())
                .SetPluginName(nameof(RevitClashDetective))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(ChecksConfig) + ".json")
                .Build<ChecksConfig>();
        }
    }

    internal class CheckSettings : ProjectSettings {
        public override string ProjectName { get; set; }
        public List<Check> Checks { get; set; } = new List<Check>();
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
