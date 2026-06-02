using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SharedParams;

using pyRevitLabs.Json;

namespace RevitRoundingOfAreas.Models;

/// <summary>
/// Класс конфигурации плагина.
/// (Если не используется удалить)
/// </summary>
internal class PluginConfig : ProjectConfig<RevitSettings> {
    /// <summary>
    /// Системное свойство конфигурации. (Не трогать)
    /// </summary>
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }

    /// <summary>
    /// Системное свойство конфигурации. (Не трогать)
    /// </summary>
    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }

    /// <summary>
    /// Метод создания конфигурации плагина.
    /// </summary>
    /// <returns>
	/// <param name="configSerializer">Сериализатор конфигурации.</param>
    /// Возвращает прочитанную конфигурацию плагина, либо созданный конфиг по умолчанию.
    /// </returns>
    public static PluginConfig GetPluginConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitRoundingOfAreas))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(PluginConfig) + ".json")
            .Build<PluginConfig>();
    }
}

internal class RevitSettings : ProjectSettings {
    /// <summary>
    /// Наименование проекта.
    /// </summary>
    public override string ProjectName { get; set; }

    /// <summary>
    /// Выбранная стадия проекта
    /// </summary>
    public ConfigSettings ConfigSettings { get; set; }
}

internal class SystemPluginConfig {
    // Название стадии по умолчанию
    public string DefaultPhaseName => "Основная планировка";

    // Количество вариантов знаков по умолчанию
    public int DefaultDigitCountRange => 3;

    // Количество знаков по умолчанию 
    public int DefaultAccuracy => 1;

    // Системный параметр площади помещений по умолчанию
    public BuiltInParameter SystemRoomAreaParamId = BuiltInParameter.ROOM_AREA;

    // Общий параметр площади помещений по умолчанию
    public RevitParam RoomAreaParam = SharedParamsConfig.Instance.RoomArea;

}
