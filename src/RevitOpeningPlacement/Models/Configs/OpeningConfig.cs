using System;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

using RevitClashDetective.Models.FilterModel;

namespace RevitOpeningPlacement.Models.Configs {
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

        public static OpeningConfig GetOpeningConfig(Document document) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            return new ProjectConfigBuilder()
                .SetSerializer(new RevitClashConfigSerializer(new OpeningSerializationBinder(), document))
                .SetPluginName(nameof(RevitOpeningPlacement))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(OpeningConfig) + ".json")
                .Build<OpeningConfig>();
        }
    }
}
