using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

namespace RevitClashDetective.Models.FilterModel {
    internal class FiltersConfig : ProjectConfig {
        public string RevitVersion { get; set; }
        public List<Filter> Filters { get; set; } = new List<Filter>();
        [JsonIgnore]
        public override string ProjectConfigPath { get; set; }
        [JsonIgnore]
        public override IConfigSerializer Serializer { get; set; }
        public static FiltersConfig GetFiltersConfig(string revitFileName, Document document) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            return new ProjectConfigBuilder()
                .SetSerializer(new RevitClashConfigSerializer(new RevitClashesSerializationBinder(), document))
                .SetPluginName(nameof(RevitClashDetective))
                .SetRelativePath(revitFileName)
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(FiltersConfig) + ".json")
                .Build<FiltersConfig>();
        }
    }
}
