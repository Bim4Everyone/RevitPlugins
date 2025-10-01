
using System;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

using RevitClashDetective.Models.FilterModel;

namespace RevitOpeningPlacement.Models.Configs;
/// <summary>
/// Настройки расстановки чистовых отверстий в файле КР
/// </summary>
internal class OpeningRealsKrConfig : ProjectConfig {
    public string RevitVersion { get; set; }
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }
    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }
    public OpeningRealKrPlacementType PlacementType { get; set; } = OpeningRealKrPlacementType.PlaceByAr;

    /// <summary>
    /// Округление габаритов отверстия в мм
    /// </summary>
    public int Rounding { get; set; } = 10;

    /// <summary>
    /// Округление отметки низа отверстия в мм
    /// </summary>
    public int ElevationRounding { get; set; } = 1;

    public static OpeningRealsKrConfig GetOpeningConfig(Document document) {
        return document is null
            ? throw new ArgumentNullException(nameof(document))
            : new ProjectConfigBuilder()
            .SetSerializer(new RevitClashConfigSerializer(new OpeningSerializationBinder(), document))
            .SetPluginName(nameof(RevitOpeningPlacement))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(OpeningRealsKrConfig) + ".json")
            .Build<OpeningRealsKrConfig>();
    }
}
