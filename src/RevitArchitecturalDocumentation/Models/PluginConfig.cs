using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitArchitecturalDocumentation.Models {
    internal class PluginConfig : ProjectConfig<PluginSettings> {
        [JsonIgnore] public override string ProjectConfigPath { get; set; }

        [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

        public static PluginConfig GetPluginConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitArchitecturalDocumentation))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(PluginConfig) + ".json")
                .Build<PluginConfig>();
        }
    }

    internal class PluginSettings : ProjectSettings {
        public override string ProjectName { get; set; }

        public bool WorkWithSheets { get; set; }
        public bool WorkWithViews { get; set; }
        public bool WorkWithSpecs { get; set; }
        public bool CreateViewsFromSelected { get; set; }
        public string SheetNamePrefix { get; set; }
        public string ViewNamePrefix { get; set; }
        public string SelectedTitleBlockName { get; set; }
        public string SelectedViewFamilyTypeName { get; set; }
        public string SelectedViewportTypeName { get; set; }
        public string SelectedFilterNameForSpecs { get; set; }
    }
}
