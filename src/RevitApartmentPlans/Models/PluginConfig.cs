using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitApartmentPlans.Models {
    internal class PluginConfig : ProjectConfig<RevitSettings> {
        [JsonIgnore] public override string ProjectConfigPath { get; set; }

        [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

        public static PluginConfig GetPluginConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitApartmentPlans))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(PluginConfig) + ".json")
                .Build<PluginConfig>();
        }
    }

    internal class RevitSettings : ProjectSettings {
        public override string ProjectName { get; set; }

        /// <summary>
        /// Наружный отступ в мм от контура квартиры
        /// </summary>
        public double OffsetMm { get; set; }

        /// <summary>
        /// Название параметра для группировки помещений по квартирам
        /// </summary>
        public string ParamName { get; set; }

        /// <summary>
        /// Id всех выбранных пользователем шаблонов видов для создания планов
        /// </summary>
        public ElementId[] ViewTemplates { get; set; }
    }
}
