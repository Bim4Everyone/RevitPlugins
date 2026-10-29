using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

namespace RevitStairRailing.Models;

/// <summary>
/// Класс конфигурации плагина.
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
            .SetPluginName(nameof(RevitStairRailing))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(PluginConfig) + ".json")
            .Build<PluginConfig>();
    }
}

/// <summary>
/// Последние значения окна плагина в проекте. Типоразмеры — по имени «Семейство : Тип», размеры — в мм.
/// </summary>
internal class RevitSettings : ProjectSettings {
    /// <summary>
    /// Наименование проекта. Системное свойство. (Не трогать)
    /// </summary>
    public override string ProjectName { get; set; } = string.Empty;

    public string PostType { get; set; }

    public string HandrailType { get; set; }

    public string RodType { get; set; }

    public string PlateType { get; set; }

    public string AnchorType { get; set; }

    public bool HasPlates { get; set; } = true;

    public bool HasAnchors { get; set; }

    public string PostHeight { get; set; } = "1056";

    public string MaxPostStep { get; set; } = "600";

    public string PostRotation { get; set; } = "90";

    public string RodOffsets { get; set; } = "234; 434; 634; 834";

    public string PlateLength { get; set; } = "100";

    public string AnchorLength { get; set; } = "95";

    public RailingSide Side { get; set; } = RailingSide.Left;

    public string EdgeOffset { get; set; } = "50";

    public bool CreateAssembly { get; set; } = true;

    public string AssemblyNamePrefix { get; set; } = "Ограждение Ом";
}
