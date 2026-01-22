using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitTagAllCategories.Models;

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
            .SetPluginName(nameof(RevitTagAllCategories))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(PluginConfig) + ".json")
            .Build<PluginConfig>();
    }
}

/// <summary>
/// Настройки проекта.
/// В настройках проекта обычно хранится выбор пользователя в основном окне плагина.
/// </summary>
/// <remarks>
/// Проектом по умолчанию является текст до первого нижнего подчеркивания.
/// <see cref="ProjectConfig" />
/// https://github.com/dosymep/dosymep.Revit/blob/master/src/dosymep.Bim4Everyone/ProjectConfigs/ProjectConfig.cs#L102
/// Если плагин работает без открытых проектов,
/// то требуется данный класс удалять из проекта,
/// как сделано в плагине RevitServerFolders
/// https://github.com/Bim4Everyone/RevitPlugins/blob/master/src/RevitServerFolders/Models/PluginConfig.cs#L8
/// </remarks>
internal class RevitSettings : ProjectSettings {
    /// <summary>
    /// Наименование проекта. Системное свойство. (Не трогать)
    /// </summary>
    public override string ProjectName { get; set; }
    
    /// <summary>
    /// Сохраняемое свойство для примера, нужно его заменить своими настройками.
    /// </summary>
    public string SaveProperty { get; set; }
}
