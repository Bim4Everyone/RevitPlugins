using System.Collections.Generic;
using System.Linq;

namespace RevitDeclarations.Models
{
    public class PrioritiesConfig {
        public PrioritiesConfig() {
            Priorities = new List<RoomPriority>();
        }

        public PrioritiesConfig(List<RoomPriority> roomPriorities) {
            Priorities = roomPriorities;
        }

        public ICollection<RoomPriority> Priorities { get; set; }
        public string[] BannedRoomNames { get; set; }

        // Возвращает приоритет из исходного списка приоритетов.
        // Если приоритет не был найден, то возращает условный приоритет
        // с номером 0, полученным именем и коэффициентом равным 1.
        public RoomPriority GetPriorityByNameOrDefault(string name) {
            return Priorities
                .Where(x => x.CheckName(name))
                .FirstOrDefault() ?? new RoomPriority(0, name);
        }
    }
}
