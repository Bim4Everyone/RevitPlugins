using System.Collections.Generic;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone;

namespace RevitClashDetective.Models.FilterModel {
    internal class FiltersConfig : ProjectConfig {
        public List<Filter> Filters { get; set; }
        public override string ProjectConfigPath { get; set; }
        public override IConfigSerializer Serializer { get; set; }
        public static FiltersConfig GetFiltersConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new RevitClashConfigSerializer())
                .SetPluginName(nameof(RevitClashDetective))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(FiltersConfig) + ".json")
                .Build<FiltersConfig>();
        }
    }
}
