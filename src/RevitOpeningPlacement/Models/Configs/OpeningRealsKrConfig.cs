
using System;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

using RevitClashDetective.Models.FilterModel;

namespace RevitOpeningPlacement.Models.Configs {
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

        public static OpeningRealsKrConfig GetOpeningConfig(Document document) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            return new ProjectConfigBuilder()
                .SetSerializer(new RevitClashConfigSerializer(new OpeningSerializationBinder(), document))
                .SetPluginName(nameof(RevitOpeningPlacement))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(OpeningRealsKrConfig) + ".json")
                .Build<OpeningRealsKrConfig>();
        }
    }
}
