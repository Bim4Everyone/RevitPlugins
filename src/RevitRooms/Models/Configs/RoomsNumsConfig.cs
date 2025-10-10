using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitRooms.Models;
public class RoomsNumsConfig : ProjectConfig<RoomsNumsSettings> {
    [JsonIgnore]
    public override string ProjectConfigPath { get; set; }

    [JsonIgnore]
    public override IConfigSerializer Serializer { get; set; }

    public static RoomsNumsConfig GetPluginConfig() {
        return new ProjectConfigBuilder()
            .SetSerializer(new ConfigSerializer())
            .SetPluginName(nameof(RevitRooms))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(RoomsNumsConfig) + ".json")
            .Build<RoomsNumsConfig>();
    }
}

public class RoomsNumsSettings : ProjectSettings {
    public override string ProjectName { get; set; }

    public Guid SelectedRoomId { get; set; }
    public string DocumentName { get; set; }

    public string StartNumber { get; set; }
    public bool IsNumFlats { get; set; }
    public bool IsNumRooms { get; set; }
    public bool IsNumRoomsGroup { get; set; }
    public bool IsNumRoomsSection { get; set; }

    public bool IsNumRoomsSectionLevels { get; set; }

    public ElementId PhaseElementId { get; set; }

    public List<ElementId> Levels { get; set; } = [];
    public List<ElementId> Groups { get; set; } = [];
    public List<ElementId> Sections { get; set; } = [];
}
