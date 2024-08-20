using System.Collections.Generic;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitClashDetective.Models.Clashes {
    internal class ClashesConfig : ProjectConfig {
        [JsonIgnore]
        public override string ProjectConfigPath { get; set; }
        [JsonIgnore]
        public override IConfigSerializer Serializer { get; set; }
        public List<ClashModel> Clashes { get; set; } = new List<ClashModel>();
        public static ClashesConfig GetClashesConfig(string revitObjectName, string congfigName) {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitClashDetective))
                .SetRelativePath(revitObjectName)
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(congfigName + ".json")
                .Build<ClashesConfig>();
        }
    }
}
