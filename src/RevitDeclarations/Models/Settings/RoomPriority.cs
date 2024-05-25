using System;

namespace RevitDeclarations.Models
{
    public class RoomPriority {
        public RoomPriority(int number, string name, bool isSummer, bool isOther = false) {
            OrdinalNumber = number;
            Name = name;
            IsSummer = isSummer;
            IsOther = isOther;
        }

        public int OrdinalNumber { get; set; }
        public string Name { get; set; }
        public string NameLower => Name.ToLower();
        public bool IsSummer { get; set; }
        public bool IsOther { get; set; }
        public int MaxRoomAmount { get; set; }

        public bool CheckName(string name) {
            if(string.Equals(name, this.Name, StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
            return false;
        }
    }
}
