using System;

using pyRevitLabs.Json;

namespace RevitDeclarations.Models
{
    public class RoomPriority {
        public RoomPriority(int number, string name) {
            OrdinalNumber = number;
            Name = name;
            IsSummer = false;
            IsLiving = false;
            IsNonConfig = false;
            AreaCoefficient = 1;
        }

        public int OrdinalNumber { get; set; }
        public string Name { get; set; }
        public bool IsSummer { get; set; }
        public bool IsLiving { get; set; }
        [JsonIgnore]
        public bool IsNonConfig { get; set; } = false;
        public double AreaCoefficient { get; set; }
        [JsonIgnore]
        public int MaxRoomAmount { get; set; }

        public bool CheckName(string name) {
            if(string.Equals(name, this.Name, StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
            return false;
        }
    }
}
