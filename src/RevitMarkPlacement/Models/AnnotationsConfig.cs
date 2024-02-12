using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitMarkPlacement.Models {
    internal class AnnotationsConfig : ProjectConfig<AnnotationsSettings> {
        [JsonIgnore]
        public override string ProjectConfigPath { get; set; }
        [JsonIgnore]
        public override IConfigSerializer Serializer { get; set; }

        public static AnnotationsConfig GetAnnotationsConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitMarkPlacement))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(AnnotationsConfig) + ".json")
                .Build<AnnotationsConfig>();
        }
    }

    internal class AnnotationsSettings : ProjectSettings {
        public override string ProjectName { get; set; }
        public int LevelCount { get; set; } = 5;
        public ElementId GlobalParameterId { get; set; }
        public SelectionMode SelectionMode { get; set; } = SelectionMode.SelectedElements;
        public LevelHeightProvider LevelHeightProvider { get; set; } = LevelHeightProvider.GlobalParameter;
        public double LevelHeight { get; set; } = 3000;
    }

    internal enum SelectionMode {
        AllElements,
        SelectedElements
    }

    internal enum LevelHeightProvider {
        UserSettings,
        GlobalParameter
    }
}
