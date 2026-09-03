using System.IO;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

namespace RevitClassifierParameters.Models;

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
            .SetPluginName(nameof(RevitClassifierParameters))
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

    public string ExcelClassifierPath { get; set; }
    public string ExcelFacadeTypePath { get; set; }
    public bool WorkWithFoundationCode { get; set; }
    public bool WorkWithConstrBelowZeroCode { get; set; }
    public bool WorkWithConstrAboveZeroCode { get; set; }
    public bool WorkWithMasonryCode { get; set; }
    public bool WorkWithRoofCode { get; set; }
    public bool WorkWithFacadeCode { get; set; }
    public bool WorkWithFacadeType { get; set; }
    public string ParamNameForFacadeType { get; set; }
}

/// <summary>
/// Системная конфигурация плагина.
/// Содержит стандартные данные, которые некорректно хранить в конфигурации проекта.
/// </summary>
internal class SystemPluginConfig {
    // Основная папка с ресурсами надстройки
    private readonly string _mainFolder =
        @"W:\Проектный институт\Отд.стандарт.BIM и RD\BIM-Ресурсы\5-Надстройки\Bim4Everyone\A101";

    // Папка с файлом Классификатора
    private readonly string _classifierDirectory =
        @"W:\Проектный институт\Проектные Группы\Типовые ТЗ\BIM-стандарт A101";

    /// <summary>
    /// Стандартный путь к файлу правил заполнения типа фасада.
    /// </summary>
    public string FacadeTypeFilePath => Path.Combine(
        _mainFolder,
        ModuleEnvironment.RevitVersion,
        "RevitClassifierParameters",
        "Правила заполнения типа фасада.xlsx");

    /// <summary>
    /// Стандартная папка с файлом Классификатора,
    /// открываемая по умолчанию в диалоге выбора файла.
    /// </summary>
    public string ClassifierDirectoryPath => _classifierDirectory;
}
