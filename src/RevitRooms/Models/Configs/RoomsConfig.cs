using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitRooms.Models;
public class RoomsConfig : ProjectConfig<RoomsSettingsConfig> {
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }

    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }

    public static RoomsConfig GetRoomsConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitRooms))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(RoomsConfig) + ".json")
            .Build<RoomsConfig>();
    }
}

public class RoomsSettingsConfig : ProjectSettings {
    public Guid SelectedRoomId { get; set; }

    public ElementId PhaseElementId { get; set; }
    public int RoundAccuracy { get; set; }
    public string RoomAccuracy { get; set; }

    public bool IsFillLevel { get; set; }
    public bool NotShowWarnings { get; set; }
    public bool IsCountRooms { get; set; }
    public bool IsSpotCalcArea { get; set; }
    public bool IsCheckRoomsChanges { get; set; }

    public List<ElementId> Levels { get; set; } = [];
    public override string ProjectName { get; set; }

}
