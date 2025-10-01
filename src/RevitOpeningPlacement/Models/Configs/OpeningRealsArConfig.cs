using System;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

using RevitClashDetective.Models.FilterModel;

namespace RevitOpeningPlacement.Models.Configs;
/// <summary>
/// Настройки расстановки чистовых отверстий в файле АР
/// </summary>
internal class OpeningRealsArConfig : ProjectConfig {
    public string RevitVersion { get; set; }
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }
    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }

    /// <summary>
    /// Округление габаритов отверстия в мм
    /// </summary>
    public int Rounding { get; set; } = 10;

    /// <summary>
    /// Округление отметки низа отверстия в мм
    /// </summary>
    public int ElevationRounding { get; set; } = 1;

    public static OpeningRealsArConfig GetOpeningConfig(Document document) {
        return document is null
            ? throw new ArgumentNullException(nameof(document))
            : new ProjectConfigBuilder()
            .SetSerializer(new RevitClashConfigSerializer(new OpeningSerializationBinder(), document))
            .SetPluginName(nameof(RevitOpeningPlacement))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(OpeningRealsArConfig) + ".json")
            .Build<OpeningRealsArConfig>();
    }
}
