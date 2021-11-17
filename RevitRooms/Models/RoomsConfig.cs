using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using pyRevitLabs.Json;

namespace RevitRooms.Models {
    public class RoomsConfig {
        public List<RoomsSettingsConfig> RoomsSettings { get; set; }

        public void AddRoomsSettingsConfig(RoomsSettingsConfig printSettingsConfig) {
            RoomsSettings = RoomsSettings ?? new List<RoomsSettingsConfig>();
            if(RoomsSettings.Count > 10) {
                foreach(int index in Enumerable.Range(0, RoomsSettings.Count - 10)) {
                    RoomsSettings.RemoveAt(index);
                }
            }

            RoomsSettings.Add(printSettingsConfig);
        }

        public RoomsSettingsConfig GetRoomsSettingsConfig(string documentName) {
            documentName = string.IsNullOrEmpty(documentName) ? "Без имени.rvt" : documentName;
            return RoomsSettings?.FirstOrDefault(item => documentName.Equals(item.DocumentName));
        }

        public static RoomsConfig GetConfig() {
            if(File.Exists(GetConfigPath())) {
                return JsonConvert.DeserializeObject<RoomsConfig>(File.ReadAllText(GetConfigPath()));
            }

            return new RoomsConfig();
        }

        public static void SaveConfig(RoomsConfig config) {
            Directory.CreateDirectory(Path.GetDirectoryName(GetConfigPath()));
            File.WriteAllText(GetConfigPath(), JsonConvert.SerializeObject(config));
        }

        private static string GetConfigPath() {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "dosymep", "RevitRooms", "RoomsConfig.json");
        }
    }

    public class RoomsSettingsConfig {
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
    }
}
