using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

namespace RevitVORChecker.Models;

/// <summary>
/// Настройки плагина: последние значения окна запуска.
/// </summary>
/// <remarks>
/// Настройки общие, а не по проекту: проверка запускается из любого документа, а проверяемые модели лежат в папке.
/// Данные проекта (из ЗнП) хранятся в папке с моделями (<see cref="Project.ProjectCardStorage"/>).
/// </remarks>
internal class PluginConfig : ProjectConfig {
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }

    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }

    /// <summary>
    /// Папка с моделями КР и выгрузками ВОР.
    /// </summary>
    public string ModelsFolder { get; set; } = string.Empty;

    /// <summary>
    /// Классификатор видов работ; пусто — найти в папке с моделями.
    /// </summary>
    public string ClassifierPath { get; set; } = string.Empty;

    /// <summary>
    /// Допуск глубины пересечения конструкций, мм.
    /// </summary>
    public string ClashToleranceMm { get; set; } = "50";

    public bool TakeScreenshots { get; set; } = true;

    public bool CheckWorksets { get; set; } = true;

    public static PluginConfig GetPluginConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitVORChecker))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(PluginConfig) + ".json")
            .Build<PluginConfig>();
    }
}
