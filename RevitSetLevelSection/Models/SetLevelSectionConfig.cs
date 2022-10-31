using System.Collections.Generic;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitSetLevelSection.Models {
    public class SetLevelSectionConfig : ProjectConfig<SetLevelSectionSettings> {
        [JsonIgnore] public override string ProjectConfigPath { get; set; }

        [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

        public static SetLevelSectionConfig GetPrintConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitSetLevelSection))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(SetLevelSectionConfig) + ".json")
                .Build<SetLevelSectionConfig>();
        }
    }

    public class SetLevelSectionSettings : ProjectSettings {
        public int LinkFileId { get; set; }
        public string BuildPart { get; set; }
        public override string ProjectName { get; set; }

        public List<ParamSettings> ParamSettings { get; set; } = new List<ParamSettings>();
    }

    public class ParamSettings {
        public int? ElementId { get; set; }
        public bool IsEnabled { get; set; }
        public string PropertyName { get; set; }
    }
}