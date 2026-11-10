using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitPackageDocumentation.Models;

/// <summary>
/// Класс конфигурации плагина. Хранит настройки по проектам Revit.
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
    /// <param name="configSerializer">Сериализатор конфигурации.</param>
    /// <returns>
    /// Возвращает прочитанную конфигурацию плагина, либо созданный конфиг по умолчанию.
    /// </returns>
    public static PluginConfig GetPluginConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitPackageDocumentation))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(PluginConfig) + ".json")
            .Build<PluginConfig>();
    }
}

/// <summary>
/// Настройки проекта: путь к последнему файлу конфигурации комплекта, с которым работал пользователь.
/// </summary>
/// <remarks>
/// Проектом по умолчанию является текст до первого нижнего подчеркивания.
/// <see cref="ProjectConfig" />
/// https://github.com/dosymep/dosymep.Revit/blob/master/src/dosymep.Bim4Everyone/ProjectConfigs/ProjectConfig.cs#L102
/// </remarks>
internal class RevitSettings : ProjectSettings {
    /// <summary>
    /// Наименование проекта. Системное свойство. (Не трогать)
    /// </summary>
    public override string ProjectName { get; set; }

    /// <summary>
    /// Путь к последнему импортированному или экспортированному файлу конфигурации комплекта (.json).
    /// При следующем запуске в этом проекте конфигурация загружается из него автоматически
    /// </summary>
    public string SheetSetDataPath { get; set; }
}
