using System.Collections.Generic;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone;
using System.IO;
using pyRevitLabs.Json;

namespace RevitClashDetective.Models.FilterModel {
    internal class FiltersConfig : ProjectConfig {
        public string RevitVersion { get; set; }
        public List<Filter> Filters { get; set; } = new List<Filter>();
        [JsonIgnore]
        public override string ProjectConfigPath { get; set; }
        [JsonIgnore]
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
