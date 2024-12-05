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

        public RoomPriority LivingRoom { get; set; }
        public RoomPriority Kitchen {get; set;}
        public RoomPriority KitchenDiningRoom {get; set;}
        public RoomPriority KitchenNiche {get; set;}
        public RoomPriority Bathroom {get; set;}
        public RoomPriority Laundry {get; set;}
        public RoomPriority Pantry {get; set;}
        public RoomPriority Cabinet {get; set;}
        public RoomPriority Corridor {get; set;}
        public RoomPriority Hall {get; set;}
        public RoomPriority Loggia {get; set;}
        public RoomPriority Balcony {get; set;}
        public RoomPriority Terrace {get; set;}

        // Возвращает приоритет из исходного списка приоритетов.
        // Если приоритет не был найден, то возращает условный приоритет
        // с номером 0, полученным именем и коэффициентом равным 1.
        public RoomPriority GetPriorityByNameOrDefault(string name) {
            return Priorities
                .Where(x => x.CheckName(name))
                .FirstOrDefault() ?? new RoomPriority(0, name);
        }

        public static PrioritiesConfig GetDefaultConfig() {
            PrioritiesConfig defaultConfig = new PrioritiesConfig();

            defaultConfig.LivingRoom = new RoomPriority(1, "Жилая комната") {
                IsLiving = true
            };
            defaultConfig.Kitchen = new RoomPriority(2, "Кухня");
            defaultConfig.KitchenDiningRoom = new RoomPriority(3, "Кухня-столовая");
            defaultConfig.KitchenNiche = new RoomPriority(4, "Кухня-ниша");
            defaultConfig.Bathroom = new RoomPriority(5, "Санузел");
            defaultConfig.Laundry = new RoomPriority(6, "Постирочная");
            defaultConfig.Pantry = new RoomPriority(7, "Гардеробная");
            defaultConfig.Cabinet = new RoomPriority(8, "Кабинет");
            defaultConfig.Corridor = new RoomPriority(9, "Коридор");
            defaultConfig.Hall = new RoomPriority(10, "Прихожая");
            defaultConfig.Loggia = new RoomPriority(11, "Лоджия") {
                IsSummer = true,
                AreaCoefficient = 0.5
            };
            defaultConfig.Balcony = new RoomPriority(12, "Балкон") {
                IsSummer = true,
                AreaCoefficient = 0.3
            };
            defaultConfig.Terrace = new RoomPriority(13, "Терраса") {
                IsSummer = true,
                AreaCoefficient = 0.3
            };

            defaultConfig.Priorities = new List<RoomPriority>() {
                defaultConfig.LivingRoom,
                defaultConfig.Kitchen,
                defaultConfig.KitchenDiningRoom,
                defaultConfig.KitchenNiche,
                defaultConfig.Bathroom,
                defaultConfig.Laundry,
                defaultConfig.Pantry,
                defaultConfig.Cabinet,
                defaultConfig.Corridor,
                defaultConfig.Hall,
                defaultConfig.Loggia,
                defaultConfig.Balcony,
                defaultConfig.Terrace,
            };

            defaultConfig.BannedRoomNames = new[] {
                "спальня",
                "детская",
                "туалет",
                "ванная",
                "уборная",
                "су",
                "с/у"
            };

            return defaultConfig;
        }

    }
}
