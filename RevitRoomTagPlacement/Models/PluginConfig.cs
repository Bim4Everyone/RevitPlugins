using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;
using pyRevitLabs.Json;

using RevitRoomTagPlacement.ViewModels;

namespace RevitRoomTagPlacement.Models {
    internal class PluginConfig : ProjectConfig<RevitSettings> {
        [JsonIgnore] public override string ProjectConfigPath { get; set; }

        [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

        public static PluginConfig GetPluginConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitRoomTagPlacement))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(PluginConfig) + ".json")
                .Build<PluginConfig>();
        }
    }

    internal class RevitSettings : ProjectSettings {
        public List<string> RoomGroups { get; set; }
        public ElementId SelectedRoomTag { get; set; }
        public GroupPlacementWay SelectedGroupPlacementWay { get; set; }
        public PositionPlacementWay SelectedPositionPlacementWay { get; set; }
        public string RoomName { get; set; }
        public double Indent { get; set; }

        public override string ProjectName { get; set; }
    }
}
