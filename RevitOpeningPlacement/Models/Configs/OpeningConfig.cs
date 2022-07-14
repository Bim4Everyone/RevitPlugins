using System;
using System.Collections.Generic;
using System.Linq;
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
        public List<MepCategory> Categories { get; set; } = new List<MepCategory>();

        public static OpeningConfig GetOpeningConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new OpeningSerializer())
                .SetPluginName(nameof(RevitOpeningPlacement))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(OpeningConfig) + ".json")
                .Build<OpeningConfig>();
        }
    }

    internal class MepCategory {
        public string Name { get; set; }
        public string ImageSource { get; set; }
        public List<Size> MinSizes { get; set; }
        public List<Offset> Offsets { get; set; }
    }

    internal class Size {
        public string Name { get; set; }
        public double Value { get; set; }
    }

    internal class Offset {
        public double From { get; set; }
        public double To { get; set; }
        public double OffsetValue { get; set; }
    }
}
