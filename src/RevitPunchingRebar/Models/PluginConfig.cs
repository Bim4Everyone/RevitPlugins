using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

namespace RevitPunchingRebar.Models;

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
    /// <param name="configSerializer">Сериализатор конфигурации.</param>
    /// <returns>Прочитанная конфигурация плагина либо новая конфигурация по умолчанию.</returns>
    public static PluginConfig GetPluginConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitPunchingRebar))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(PluginConfig) + ".json")
            .Build<PluginConfig>();
    }
}

internal class RevitSettings : ProjectSettings {
    /// <summary>
    /// Наименование проекта. Системное свойство. (Не трогать)
    /// </summary>
    public override string ProjectName { get; set; }

    public FrameParameterSettings ParameterSettings { get; set; }

    public int SlabRebarDiameter { get; set; }
    public bool IsRebarCoverFromUser { get; set; }
    public int RebarCoverTop { get; set; }
    public int RebarCoverBottom { get; set; }
    public string FamilyName { get; set; }
    public string FamilyType { get; set; }
    public bool SwapFrameElevations { get; set; }
    public int StirrupDiameter { get; set; }
    public RebarClass? StirrupRebarClass { get; set; }
    public int StirrupStep { get; set; }

    // Полная ширина в конфигурациях до перехода на ввод шага.
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public int? FrameWidth { get; set; }
    public int? WidthStep { get; set; }
    public int? LongRebarDiameter { get; set; }
    public int? LongRebarAnchorageLength { get; set; }
    public int? StirrupBendLength { get; set; }

    public int GetWidthStepMm() {
        if(WidthStep is int widthStep) {
            return widthStep;
        }

        int stirrupDiameter = StirrupDiameter > 0 ? StirrupDiameter : DefaultSettings.StirrupDiameterMm;
        int longRebarDiameter = LongRebarDiameter ?? DefaultSettings.LongRebarDiameterMm;
        int previousStep = FrameWidth.GetValueOrDefault() - stirrupDiameter - longRebarDiameter;
        return previousStep > 0 ? previousStep : DefaultSettings.WidthStepMm;
    }
}
