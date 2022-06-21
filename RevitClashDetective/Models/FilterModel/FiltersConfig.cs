using System.Collections.Generic;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone;
using System.IO;

namespace RevitClashDetective.Models.FilterModel {
    internal class FiltersConfig : ProjectConfig {
        public List<Filter> Filters { get; set; } = new List<Filter>();
        public override string ProjectConfigPath { get; set; }
        public override IConfigSerializer Serializer { get; set; }
        public static FiltersConfig GetFiltersConfig(string revitFileName) {
            return new ProjectConfigBuilder()
                .SetSerializer(new RevitClashConfigSerializer())
                .SetRelativePath(Path.Combine(nameof(RevitClashDetective), revitFileName))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(FiltersConfig) + ".json")
                .Build<FiltersConfig>();
        }
    }
}
