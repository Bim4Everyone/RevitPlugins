using System;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

namespace RevitOpeningPlacement.Models.Configs {
    internal class OpeningConfig : ProjectConfig {
        public string RevitVersion { get; set; }
        [JsonIgnore]
        public override string ProjectConfigPath { get; set; }
        [JsonIgnore]
        public override IConfigSerializer Serializer { get; set; }
        public MepCategoryCollection Categories { get; set; } = new MepCategoryCollection();

        public static OpeningConfig GetOpeningConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new OpeningSerializer())
                .SetPluginName(nameof(RevitOpeningPlacement))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(OpeningConfig) + ".json")
                .Build<OpeningConfig>();
        }
    }
}
