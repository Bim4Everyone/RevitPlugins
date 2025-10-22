using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

namespace RevitCreateViewSheet.Models {
    internal class PluginConfig : ProjectConfig<RevitSettings> {
        [JsonIgnore]
        public override string ProjectConfigPath { get; set; }

        [JsonIgnore]
        public override IConfigSerializer Serializer { get; set; }

        public static PluginConfig GetPluginConfig(IConfigSerializer configSerializer) {
            return new ProjectConfigBuilder()
                .SetSerializer(configSerializer)
                .SetPluginName(nameof(RevitCreateViewSheet))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(PluginConfig) + ".json")
                .Build<PluginConfig>();
        }
    }

    internal class RevitSettings : ProjectSettings {
        public override string ProjectName { get; set; }

        public string FileDialogInitialDirectory { get; set; }

        /// <summary>
        /// Id типоразмера основной надписи для генерации новых листов
        /// </summary>
        public ElementId AddSheetsTitleBlock { get; set; } = ElementId.InvalidElementId;
    }
}
