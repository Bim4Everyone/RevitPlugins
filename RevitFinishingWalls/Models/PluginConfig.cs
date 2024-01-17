using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

using RevitFinishingWalls.Models.Enums;

namespace RevitFinishingWalls.Models {
    /// <summary>
    /// Настройки расстановки
    /// </summary>
    internal class PluginConfig : ProjectConfig {
        [JsonIgnore] public override string ProjectConfigPath { get; set; }

        [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

        /// <summary>
        /// Тип отделочной стены
        /// </summary>
        [JsonIgnore] public WallType WallType { get; set; }

        /// <summary>
        /// Высота стен, заданная пользователем
        /// </summary>
        public int WallHeightByUser { get; set; } = 3000;

        /// <summary>
        /// Режим выбора помещений для обработки
        /// </summary>
        public RoomGetterMode RoomGetterMode { get; set; }

        /// <summary>
        /// Режим задания высоты стен
        /// </summary>
        public WallHeightMode WallHeightMode { get; set; }

        /// <summary>
        /// Отступ низа стены от уровня
        /// </summary>
        public int WallBaseOffset { get; set; }


        public static PluginConfig GetPluginConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitFinishingWalls))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(PluginConfig) + ".json")
                .Build<PluginConfig>();
        }
    }
}
