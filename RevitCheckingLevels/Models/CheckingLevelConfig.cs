using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone;
using dosymep.Serializers;
using pyRevitLabs.Json;

namespace RevitCheckingLevels.Models {
    internal class CheckingLevelConfig : ProjectConfig<CheckingLevelSettings> {
        [JsonIgnore] public override string ProjectConfigPath { get; set; }

        [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

        public static CheckingLevelConfig GetCheckingLevelConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitCheckingLevels))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(CheckingLevelConfig) + ".json")
                .Build<CheckingLevelConfig>();
        }
    }

    internal class CheckingLevelSettings : ProjectSettings {
        public ElementId LinkTypeId { get; set; }
        public override string ProjectName { get; set; }
    }
}
