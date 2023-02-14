using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitCopingZones.Models {
    internal class CopingZonesConfig :ProjectConfig<CopingZonesSettings> {
        [JsonIgnore] public override string ProjectConfigPath { get; set; }

        [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

        public static CopingZonesConfig GetCheckingLevelConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitCopingZones))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(CopingZonesConfig) + ".json")
                .Build<CopingZonesConfig>();
        }
    }

    internal class CopingZonesSettings : ProjectSettings {
        public override string ProjectName { get; set; }
    }
}