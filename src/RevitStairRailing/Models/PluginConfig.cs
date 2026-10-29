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

    public string PostHeight { get; set; } = "1056";

    public string MaxPostStep { get; set; } = "600";

    public string PostRotation { get; set; } = "90";

    public string RodOffsets { get; set; } = "234; 434; 634; 834";

    public PostMount FlightMount { get; set; } = PostMount.Top;

    public PostMount LandingMount { get; set; } = PostMount.Top;

    public string PlateFamily { get; set; }

    public string BasePlateThickness { get; set; } = "6";

    public string BasePlateLength { get; set; } = "100";

    public string BasePlateWidth { get; set; } = "100";

    public bool HasAnchors { get; set; } = true;

    public string AnchorType { get; set; }

    public AnchorLayout AnchorLayout { get; set; } = AnchorLayout.Triangle;

    public string AnchorEdgeDistance { get; set; } = "20";

    public string CheekThickness { get; set; } = "6";

    public string CheekWidth { get; set; } = "60";

    public string CheekHeight { get; set; } = "120";

    public string CheekGap { get; set; } = "30";

    public CheekOrientation CheekOrientation { get; set; } = CheekOrientation.AlongRailing;

    public RailingSide Side { get; set; } = RailingSide.Left;

    public string EdgeOffset { get; set; } = "50";

    public bool CreateAssembly { get; set; } = true;

    public string AssemblyNamePrefix { get; set; } = "Ограждение Ом";
}
