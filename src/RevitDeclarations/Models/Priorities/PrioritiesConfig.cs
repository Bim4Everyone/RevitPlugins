using System.Collections.Generic;
using System.Linq;

namespace RevitDeclarations.Models;
public class PrioritiesConfig {
    public PrioritiesConfig() {
        Priorities = [];
    }

    public PrioritiesConfig(List<RoomPriority> roomPriorities) {
        Priorities = roomPriorities;
    }

    public ICollection<RoomPriority> Priorities { get; set; }
    public string[] BannedRoomNames { get; set; }

    public RoomPriority LivingRoom { get; set; }
    public RoomPriority Kitchen { get; set; }
    public RoomPriority KitchenDiningRoom { get; set; }
    public RoomPriority KitchenNiche { get; set; }
    public RoomPriority Bathroom { get; set; }
    public RoomPriority Laundry { get; set; }
    public RoomPriority Pantry { get; set; }
    public RoomPriority Cabinet { get; set; }
    public RoomPriority Corridor { get; set; }
    public RoomPriority Hall { get; set; }
    public RoomPriority Loggia { get; set; }
    public RoomPriority Balcony { get; set; }
    public RoomPriority Terrace { get; set; }

    // Возвращает приоритет из исходного списка приоритетов.
    // Если приоритет не был найден, то возращает условный приоритет
    // с номером 0, полученным именем и коэффициентом равным 1.
    public RoomPriority GetPriorityByNameOrDefault(string name) {
        return Priorities
            .Where(x => x.CheckName(name))
            .FirstOrDefault() ?? new RoomPriority(0, name);
    }

    public static PrioritiesConfig GetDefaultConfig() {
        var defaultConfig = new PrioritiesConfig {
            LivingRoom = new RoomPriority(1, "Жилая комната") {
                IsLiving = true
            },
            Kitchen = new RoomPriority(2, "Кухня"),
            KitchenDiningRoom = new RoomPriority(3, "Кухня-столовая"),
            KitchenNiche = new RoomPriority(4, "Кухня-ниша"),
            Bathroom = new RoomPriority(5, "Санузел"),
            Laundry = new RoomPriority(6, "Постирочная"),
            Pantry = new RoomPriority(7, "Гардеробная"),
            Cabinet = new RoomPriority(8, "Кабинет"),
            Corridor = new RoomPriority(9, "Коридор"),
            Hall = new RoomPriority(10, "Прихожая"),
            Loggia = new RoomPriority(11, "Лоджия") {
                IsSummer = true,
                AreaCoefficient = 0.5
            },
            Balcony = new RoomPriority(12, "Балкон") {
                IsSummer = true,
                AreaCoefficient = 0.3
            },
            Terrace = new RoomPriority(13, "Терраса") {
                IsSummer = true,
                AreaCoefficient = 0.3
            }
        };

        defaultConfig.Priorities = [
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
        ];

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
