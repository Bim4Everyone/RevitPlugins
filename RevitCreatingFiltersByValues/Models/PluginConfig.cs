using System.Collections.Generic;
using System.Collections.ObjectModel;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitCreatingFiltersByValues.Models {
    internal class PluginConfig : ProjectConfig<PluginSettings> {
        [JsonIgnore] public override string ProjectConfigPath { get; set; }

        [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

        public static PluginConfig GetPluginConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitCreatingFiltersByValues))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(PluginConfig) + ".json")
                .Build<PluginConfig>();
        }
    }

    internal class PluginSettings : ProjectSettings {
        public override string ProjectName { get; set; }

        public bool OverrideByPattern { get; set; }
        public bool OverrideByColor { get; set; }
        public bool OverridingWithFilters { get; set; }
        public bool OverridingWithRepaint { get; set; }
        public ObservableCollection<ColorHelper> Colors { get; set; }
        public List<string> PatternNames { get; set; }
    }
}