using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using pyRevitLabs.Json;

namespace RevitRooms.Models {
    public class RoomsNumsConfig {
        public List<RoomsNumsSettings> RoomsSettings { get; set; }

        public void AddRoomsNumsSettingsConfig(RoomsNumsSettings printSettingsConfig) {
            RoomsSettings = RoomsSettings ?? new List<RoomsNumsSettings>();
            if(RoomsSettings.Count > 10) {
                foreach(int index in Enumerable.Range(0, RoomsSettings.Count - 10)) {
                    RoomsSettings.RemoveAt(index);
                }
            }

            RoomsSettings.Add(printSettingsConfig);
        }

        public RoomsNumsSettings GetRoomsNumsSettingsConfig(string documentName) {
            documentName = string.IsNullOrEmpty(documentName) ? "Без имени.rvt" : documentName;
            return RoomsSettings?.FirstOrDefault(item => documentName.Equals(item.DocumentName));
        }

        public static RoomsNumsConfig GetConfig() {
            if(File.Exists(GetConfigPath())) {
                return JsonConvert.DeserializeObject<RoomsNumsConfig>(File.ReadAllText(GetConfigPath()));
            }

            return new RoomsNumsConfig();
        }

        public static void SaveConfig(RoomsNumsConfig config) {
            Directory.CreateDirectory(Path.GetDirectoryName(GetConfigPath()));
            File.WriteAllText(GetConfigPath(), JsonConvert.SerializeObject(config));
        }

        private static string GetConfigPath() {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "dosymep", "RevitRooms", "RoomsNumsConfig.json");
        }
    }

    public class RoomsNumsSettings {
        public Guid SelectedRoomId { get; set; }
        public string DocumentName { get; set; }

        public string StartNumber { get; set; }
        public bool IsNumFlats { get; set; }
        public bool IsNumRooms { get; set; }
        public bool IsNumRoomsGroup { get; set; }
        public bool IsNumRoomsSection { get; set; }

        public int PhaseElementId { get; set; }

        public List<int> Levels { get; set; } = new List<int>();
        public List<int> Groups { get; set; } = new List<int>();
        public List<int> Sections { get; set; } = new List<int>();
    }

}
