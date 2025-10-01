using System;
using System.IO;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

using RevitClashDetective.Models.FilterModel;

namespace RevitOpeningPlacement.Models.Configs;
/// <summary>
/// Настройки расстановки заданий на отверстия в файле ВИС
/// </summary>
internal class OpeningConfig : ProjectConfig {
    public string RevitVersion { get; set; }
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }
    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }
    public MepCategoryCollection Categories { get; set; } = new MepCategoryCollection();

    /// <summary>
    /// Округление размеров объединенных заданий на отверстия в мм
    /// </summary>
    public int UnitedTasksSizeRounding { get; set; } = 10;

    /// <summary>
    /// Округление отметки объединенных заданий на отверстия в мм
    /// </summary>
    public int UnitedTasksElevationRounding { get; set; } = 10;

    /// <summary>
    /// Флаг для вывода ошибок расстановки заданий на отверстия пользователю
    /// </summary>
    public bool ShowPlacingErrors { get; set; } = false;

    /// <summary>
    /// Название настроек
    /// </summary>
    public string Name { get; set; } = "по умолчанию";

    public static OpeningConfig GetOpeningConfig(Document document) {
        if(document is null) { throw new ArgumentNullException(nameof(document)); }

        OpeningConfig config;
        try {
            config = GetOverriddenBuilder(document)
                .Build<OpeningConfig>();
        } catch(JsonException) {
            try {
                config = GetDefaultBuilder(document)
                    .Build<OpeningConfig>();
            } catch(JsonException) {
                // файл конфига по умолчанию был сохранен с использованием параметра,
                // который отсутствует в активном документе
                config = new OpeningConfig() {
                    ProjectConfigPath = GetDefaultPath(),
                    Serializer = new RevitClashConfigSerializer(new OpeningSerializationBinder(), document)
                };
            }
        }
        var defaultCollection = new MepCategoryCollection();
        if(config.Categories.Count == defaultCollection.Categories.Count) {
            return config;
        } else {
            config.Categories = defaultCollection;
            return config;
        }
    }

    private static ProjectConfigBuilder GetOverriddenBuilder(Document document) {
        var builder = GetDefaultBuilder(document);
        string configPath = MepConfigPath.GetMepConfigPath(document).OpeningConfigPath;
        if(!string.IsNullOrWhiteSpace(configPath) && File.Exists(configPath)) {
            builder.SetProjectConfigPath(configPath);
        }
        return builder;
    }

    private static ProjectConfigBuilder GetDefaultBuilder(Document document) {
        return new ProjectConfigBuilder()
             .SetSerializer(new RevitClashConfigSerializer(new OpeningSerializationBinder(), document))
             .SetPluginName(nameof(RevitOpeningPlacement))
             .SetRevitVersion(ModuleEnvironment.RevitVersion)
             .SetProjectConfigName(nameof(OpeningConfig) + ".json");
    }

    private static string GetDefaultPath() {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            "dosymep",
            ModuleEnvironment.RevitVersion,
            nameof(RevitOpeningPlacement),
            nameof(OpeningConfig) + ".json");
    }
}
