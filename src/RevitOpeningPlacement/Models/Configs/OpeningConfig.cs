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

        /// <summary>
        /// Флаг для вывода ошибок расстановки заданий на отверстия пользователю
        /// </summary>
        public bool ShowPlacingErrors { get; set; } = false;

        public static OpeningConfig GetOpeningConfig(Document document) {
            if(document is null) { throw new ArgumentNullException(nameof(document)); }

            var config = new ProjectConfigBuilder()
                .SetSerializer(new RevitClashConfigSerializer(new OpeningSerializationBinder(), document))
                .SetPluginName(nameof(RevitOpeningPlacement))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(OpeningConfig) + ".json")
                .Build<OpeningConfig>();
            MepCategoryCollection defaultCollection = new MepCategoryCollection();
            if(config.Categories.Count == defaultCollection.Categories.Count) {
                return config;
            } else {
                config.Categories = defaultCollection;
                return config;
            }
        }
    }
}
