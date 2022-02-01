using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitRooms.Models {
    public class RoomsConfig : ProjectConfig<RoomsSettingsConfig> {
        [JsonIgnore]
        public override string ProjectConfigPath { get; set; }

        [JsonIgnore]
        public override IConfigSerializer Serializer { get; set; }

        public static RoomsConfig GetRoomsConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitRooms))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(RoomsConfig) + ".json")
                .Build<RoomsConfig>();
        }
    }

    public class RoomsSettingsConfig : ProjectSettings {
        public Guid SelectedRoomId { get; set; }
        public string DocumentName { get; set; }

        public int PhaseElementId { get; set; }
        public int RoundAccuracy { get; set; }
        public string RoomAccuracy { get; set; }

        public bool NotShowWarnings { get; set; }
        public bool IsCountRooms { get; set; }
        public bool IsSpotCalcArea { get; set; }
        public bool IsCheckRoomsChanges { get; set; }

        public List<int> Levels { get; set; } = new List<int>();
        public override string ProjectName { get; set; }
    }
}
