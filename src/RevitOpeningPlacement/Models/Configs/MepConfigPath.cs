using System;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

using RevitClashDetective.Models.FilterModel;

namespace RevitOpeningPlacement.Models.Configs;
/// <summary>
/// Класс для хранения пути к файлу настроек расстановки заданий на отверстия от вИС
/// </summary>
internal class MepConfigPath : ProjectConfig {
    public string RevitVersion { get; set; }
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }
    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }

    /// <summary>
    /// Путь к файлу настроек расстановки заданий на отверстия от ВИС
    /// </summary>
    public string OpeningConfigPath { get; set; }

    public static MepConfigPath GetMepConfigPath(Document document) {
        return document is null
            ? throw new ArgumentNullException(nameof(document))
            : new ProjectConfigBuilder()
            .SetSerializer(new RevitClashConfigSerializer(new OpeningSerializationBinder(), document))
            .SetPluginName(nameof(RevitOpeningPlacement))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(MepConfigPath) + ".json")
            .Build<MepConfigPath>();
    }
}
