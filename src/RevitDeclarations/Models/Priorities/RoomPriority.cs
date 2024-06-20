using System;

namespace RevitDeclarations.Models
{
    public class RoomPriority {
        public RoomPriority(int number, string name) {
            OrdinalNumber = number;
            Name = name;
            IsSummer = false;
            IsLiving = false;
            IsOther = false;
            AreaCoefficient = 1;
        }

        public int OrdinalNumber { get; set; }
        public string Name { get; set; }
        public string NameLower => Name.ToLower();
        public bool IsSummer { get; set; }
        public bool IsLiving { get; set; }
        public bool IsOther { get; set; }
        public int MaxRoomAmount { get; set; }
        public double AreaCoefficient { get; set; }

        public bool CheckName(string name) {
            if(string.Equals(name, this.Name, StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
            return false;
        }
    }
}
