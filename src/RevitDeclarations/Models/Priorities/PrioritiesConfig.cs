using System.Collections.Generic;

namespace RevitDeclarations.Models
{
    public class PrioritiesConfig {
        private readonly string[] _bannedNames;

        private readonly RoomPriority _livingRoom;
        private readonly RoomPriority _kitchen;
        private readonly RoomPriority _kitchenNiche;
        private readonly RoomPriority _bathroom;
        private readonly RoomPriority _laundry;
        private readonly RoomPriority _pantry;
        private readonly RoomPriority _cabinet;
        private readonly RoomPriority _corridor;
        private readonly RoomPriority _hall;
        private readonly RoomPriority _loggia;
        private readonly RoomPriority _balcony;
        private readonly RoomPriority _terrace;

        private readonly ICollection<RoomPriority> _priorities;

        public PrioritiesConfig() {
            _livingRoom = new RoomPriority(1, "Жилая комната") { 
                IsLiving = true 
            };
            _kitchen = new RoomPriority(2, "Кухня");
            _kitchenNiche = new RoomPriority(3, "Кухня-ниша");
            _bathroom = new RoomPriority(4, "Санузел");
            _laundry = new RoomPriority(5, "Постирочная");
            _pantry = new RoomPriority(6, "Гардеробная");
            _cabinet = new RoomPriority(7, "Кабинет");
            _corridor = new RoomPriority(8, "Коридор");
            _hall = new RoomPriority(9, "Прихожая");
            _loggia = new RoomPriority(10, "Лоджия") {
                IsSummer = true,
                AreaCoefficient = 0.5
            };
            _balcony = new RoomPriority(11, "Балкон") {
                IsSummer = true,
                AreaCoefficient = 0.3
            };
            _terrace = new RoomPriority(12, "Терраса") {
                IsSummer = true,
                AreaCoefficient = 0.3
            };

            _priorities = new List<RoomPriority>() {
                _livingRoom,
                _kitchen,
                _kitchenNiche,
                _bathroom,
                _laundry,
                _pantry,
                _cabinet,
                _corridor,
                _hall,
                _loggia,
                _balcony,
                _terrace
            };

            _bannedNames = new[] {
                "спальня",
                "детская",
                "туалет",
                "ванная",
                "уборная",
                "су",
                "с/у"
            };
        }

        public RoomPriority LivingRoom => _livingRoom;
        public RoomPriority Kitchen => _kitchen;
        public RoomPriority KitchenNiche => _kitchenNiche;
        public RoomPriority Bathroom => _bathroom;
        public RoomPriority Laundry => _laundry;
        public RoomPriority Pantry => _pantry;
        public RoomPriority Cabinet => _cabinet;
        public RoomPriority Corridor => _corridor;
        public RoomPriority Hall => _hall;
        public RoomPriority Loggia => _loggia;
        public RoomPriority Balcony => _balcony;
        public RoomPriority Terrace => _terrace;

        public ICollection<RoomPriority> Priorities => _priorities;
        public string[] BannedRoomNames => _bannedNames;
    }
}
