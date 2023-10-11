using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitSetLevelSection.Models {
    public class PluginConfig : ProjectConfig<RevitSettings> {
        [JsonIgnore] public override string ProjectConfigPath { get; set; }

        [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

        public static PluginConfig GetPluginConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitSetLevelSection))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(PluginConfig) + ".json")
                .Build<PluginConfig>();
        }
    }

    public class RevitSettings : ProjectSettings {
        public string BuildPart { get; set; }
        public override string ProjectName { get; set; }

        public List<ParamSettings> ParamSettings { get; set; } = new List<ParamSettings>();
    }

    public class ParamSettings {
        public int? BuildPartId { get; set; }
        public ElementId DesignOptionId { get; set; }
        public string ParamId { get; set; }
        public bool IsEnabled { get; set; }
    }
}