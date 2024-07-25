using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

using RevitFinishingWalls.Models.Enums;

namespace RevitFinishingWalls.Models {
    /// <summary>
    /// Настройки расстановки отделочных стен
    /// </summary>
    internal class PluginConfig : ProjectConfig<RevitSettings> {
        [JsonIgnore] public override string ProjectConfigPath { get; set; }

        [JsonIgnore] public override IConfigSerializer Serializer { get; set; }


        public static PluginConfig GetPluginConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitFinishingWalls))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(PluginConfig) + ".json")
                .Build<PluginConfig>();
        }
    }

    internal class RevitSettings : ProjectSettings {
        public override string ProjectName { get; set; }

        /// <summary>
        /// Тип отделочной стены
        /// </summary>
        public ElementId WallTypeId { get; set; } = ElementId.InvalidElementId;

        /// <summary>
        /// Отметка верха стены от уровня, заданная пользователем в мм
        /// </summary>
        public double WallElevationMm { get; set; } = 3000;

        /// <summary>
        /// Режим выбора помещений для обработки
        /// </summary>
        public RoomGetterMode RoomGetterMode { get; set; } = RoomGetterMode.RoomsOnActiveView;

        /// <summary>
        /// Режим задания верхней отметки стен от уровня
        /// </summary>
        public WallElevationMode WallElevationMode { get; set; } = WallElevationMode.HeightByRoom;

        /// <summary>
        /// Отступ низа стены от уровня в мм
        /// </summary>
        public double WallBaseOffsetMm { get; set; } = 0;

        /// <summary>
        /// Смещение стены внутрь помещения
        /// </summary>
        public double WallSideOffsetMm { get; set; } = 0;
    }
}
