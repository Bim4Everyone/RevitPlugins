using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.Models {
    internal class ChecksConfig : ProjectConfig {
        public override string ProjectConfigPath { get; set; }
        public override IConfigSerializer Serializer { get; set; }
        public List<Check> Checks { get; set; } = new List<Check>();
        public static ChecksConfig GetFiltersConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new RevitClashConfigSerializer())
                .SetPluginName(nameof(RevitClashDetective))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(ChecksConfig) + ".json")
                .Build<ChecksConfig>();
        }
    }

    internal class Check {
        public string Name { get; set; }
        public List<string> MainFilters { get; set; } = new List<string>();
        public List<string> OtherFilters { get; set; } = new List<string>();
    }
}
