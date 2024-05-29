using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;

namespace RevitDeclarations.Models {
    internal class UtpCalculator {
        private readonly DeclarationProject _project;
        private readonly DeclarationSettings _settings;

        private readonly PrioritiesConfig _priorities;

        private readonly bool _hasNullAreas;
        private readonly bool _hasBannedNames;

        // Гардеробные с дверью в жилую комнату. Для УТП Гардеробная
        private readonly List<ElementId> _pantriesWithBedroom;
        // Мастер-спальни 
        private readonly List<ElementId> _masterBedrooms;
        // Санузлы, относящиеся к мастер спальням
        private readonly List<ElementId> _masterBathrooms;

        private readonly List<ElementId> _roomsWithBathFamily;

        public UtpCalculator(DeclarationProject project, DeclarationSettings settings) {
            _project = project;
            _settings = settings;

            _hasNullAreas = CheckUtpNullAreas();
            _hasBannedNames = CheckUtpRoomNames();

            _priorities = _settings.PrioritiesConfig;

            var bedroomBathroom = GetRoomsByDoors(_priorities.LivingRoom.NameLower, _priorities.Bathroom.NameLower);
            List<ElementId> bedroomsWithBathroom = bedroomBathroom[_priorities.LivingRoom.NameLower];

            var pantryBedroom = GetRoomsByDoors(_priorities.Pantry.NameLower, _priorities.LivingRoom.NameLower);
            var pantryBathroom = GetRoomsByDoors(_priorities.Pantry.NameLower, _priorities.Bathroom.NameLower);
            _pantriesWithBedroom = pantryBedroom[_priorities.Pantry.NameLower];
            List<ElementId> pantriesWithBathroom = pantryBathroom[_priorities.Pantry.NameLower];
            List<ElementId> masterPantries = _pantriesWithBedroom.Intersect(pantriesWithBathroom).ToList();

            var bedroomPantry = GetRoomsByDoors(_priorities.LivingRoom.NameLower, masterPantries);
            var bedroomsWithPantryAndBathroom = bedroomPantry[_priorities.LivingRoom.NameLower];
            _masterBedrooms = bedroomsWithBathroom.Concat(bedroomsWithPantryAndBathroom).ToList();

            List<ElementId> bathroomsWithBedroom = bedroomBathroom[_priorities.Bathroom.NameLower];
            var bathroomPantry = GetRoomsByDoors(_priorities.Bathroom.NameLower, masterPantries);
            var bathroomsWithMasterPantry = bathroomPantry[_priorities.Bathroom.NameLower];
            _masterBathrooms = bathroomsWithBedroom.Concat(bathroomsWithMasterPantry).ToList();

            _roomsWithBathFamily = GetRoomsWithBath();
        }

        private bool CheckUtpRoomNames() {
            return _project
                .Rooms
                .Where(x => _settings.BannedRoomNames.Contains(x.Name))
                .Any();
        }

        private bool CheckUtpNullAreas() {
            return _project
                .Rooms
                .Where(x => x.AreaRevit < 0.1)
                .Any();
        }

        private Dictionary<string, List<ElementId>> GetRoomsByDoors(string name1, string name2) {
            BuiltInParameter bltParam = BuiltInParameter.ROOM_NAME;
            List<FamilyInstance> doors = _project.GetDoors();

            Dictionary<string, List<ElementId>> roomByNames = new Dictionary<string, List<ElementId>> {
                [name1] = new List<ElementId>(),
                [name2] = new List<ElementId>()
            };

            foreach(var door in doors) {
                Room room1 = door.get_FromRoom(_project.Phase);
                Room room2 = door.get_ToRoom(_project.Phase);
                string roomName1 = room1?.get_Parameter(bltParam).AsString().ToLower();
                string roomName2 = room2?.get_Parameter(bltParam).AsString().ToLower();

                if(roomName1 == name1 && roomName2 == name2) {
                    roomByNames[name1].Add(room1.Id);
                    roomByNames[name2].Add(room2.Id);
                }

                if(roomName1 == name2 && roomName2 == name1) {
                    roomByNames[name1].Add(room2.Id);
                    roomByNames[name2].Add(room1.Id);
                }
            }

            return roomByNames;
        }

        private Dictionary<string, List<ElementId>> GetRoomsByDoors(string name1, List<ElementId> rooms) {
            BuiltInParameter bltParam = BuiltInParameter.ROOM_NAME;
            List<FamilyInstance> doors = _project.GetDoors();

            Dictionary<string, List<ElementId>> roomByNames = new Dictionary<string, List<ElementId>> {
                [name1] = new List<ElementId>(),
            };

            foreach(var door in doors) {
                Room room1 = door.get_FromRoom(_project.Phase);
                Room room2 = door.get_ToRoom(_project.Phase);
                string roomName1 = room1?.get_Parameter(bltParam).AsString().ToLower();
                string roomName2 = room2?.get_Parameter(bltParam).AsString().ToLower();

                if(roomName1 == name1 && rooms.Contains(room2.Id)) {
                    roomByNames[name1].Add(room1.Id);
                }

                if(roomName2 == name1 && rooms.Contains(room1.Id)) {
                    roomByNames[name1].Add(room2.Id);
                }
            }

            return roomByNames;
        }

        private List<ElementId> GetRoomsWithBath() {
            return _project
                .GetBathInstances()
                .Select(x => x.get_Room(_project.Phase))
                .Distinct()
                .Select(x => x.Id)
                .ToList();
        }

        // УТП Хайфлет.
        // Все помещения в квартире имеют высоту от 3400 мм.
        public string CalculateHighflat(Apartment apartment) {
            foreach(var room in apartment.Rooms) {
                double height = room.GetLengthParamValue(_settings.RoomsHeightParam, _settings.Accuracy);
                if(height < 3400) {
                    return "Нет";
                }
            }
            return "Да";
        }

        // УТП Две ванны.
        // В квартире две и более ванны (или душевые) в разных помещениях санузлов.
        // Помещения санузлов, которые относятся к мастер-спальням не учитываются.
        public string CalculateTwoBathes(Apartment apartment) {
            if(_hasBannedNames) {
                return "Ошибка имен помещений";
            }

            int bathAmount = 0;

            bathAmount = apartment.Rooms
                .Select(x => x.RevitRoom.Id)
                .Where(x => !_masterBathrooms.Contains(x))
                .Where(x => _roomsWithBathFamily.Contains(x))
                .Count();

            if(bathAmount > 1) {
                return "Да";
            }

            return "Нет";
        }

        // УТП Лоджия/балкон.
        // В квартире есть минимум одно помещение с именем "Лоджия" или "Балкон".
        public string CalculateBalcony(Apartment apartment) {

            int balconies = apartment.GetRoomsByPrior(_priorities.Balcony).Count;
            int loggies = apartment.GetRoomsByPrior(_priorities.Loggia).Count;

            if(balconies + loggies > 0) {
                return "Да";
            }
            return "Нет";
        }

        // УТП Доп. летние помещения.
        // В квартире есть минимум два помещения с именем "Лоджия" или "Балкон".
        public string CalculateExtraSummerRooms(Apartment apartment) {
            int balconies = apartment.GetRoomsByPrior(_priorities.Balcony).Count;
            int loggies = apartment.GetRoomsByPrior(_priorities.Loggia).Count;

            if(balconies + loggies > 1) {
                return "Да";
            }
            return "Нет";
        }

        // УТП Мастер-спальня.
        // В квартире минимум одна жилая комната, соединенная с санузлом.
        // Жилая комната может иметь дверь непосредственно в санузел
        // или сначала в гардеробную, а гардеробная уже в санузел.
        public string CalculateMasterBedroom(Apartment apartment) {
            if(_hasBannedNames) {
                return "Ошибка имен помещений";
            }

            foreach(var room in apartment.Rooms) {
                if(_masterBedrooms.Contains(room.RevitRoom.Id)) {
                    return "Да";
                }
            }
            return "Нет";
        }

        // УТП Терраса.
        // В квартире есть минимум одно помещение с именем "Терраса".
        public string CalculateTerrace(Apartment apartment) {
            int amount = apartment.GetRoomsByPrior(_priorities.Terrace).Count;

            if(amount > 0) {
                return "Да";
            }
            return "Нет";
        }

        // УТП Увеличенная площадь балкона/лоджии.
        // Есть минимум одно помещение с именем "Лоджия" или "Балкон"
        // с размерами не менее 2000х1200 мм.
        public string CalculateExtraBalconyArea(Apartment apartment) {
            if(_hasNullAreas) {
                return "Ошибка нулевых площадей";
            }

            List<RoomElement> summerRooms = apartment.GetRoomsByPrior(_priorities.Balcony);
            summerRooms.AddRange(apartment.GetRoomsByPrior(_priorities.Loggia));
            List<Room> summerRevitRooms = summerRooms.Select(x => x.RevitRoom).ToList();

            if(summerRooms.Any()) {
                return ContourChecker
                    .CheckAnyRoomSizes(summerRevitRooms, _settings.Accuracy, 2000, 1200)
                    .GetDescription();
            } else {
                return "Нет";
            }
        }

        // УТП Гардеробная.
        // Наличие в квартире минимум одного помещения с именем "Гардеробная".
        // Одна сторона помещения должна быть не менее 1000 мм, площадь от 1,5 м2.
        // Помещения, которые связаны дверью с жилой комнатой не учитываются.
        public string CalculatePantry(Apartment apartment) {
            if(_hasNullAreas) {
                return "Ошибка нулевых площадей";
            }
            if(_hasBannedNames) {
                return "Ошибка имен помещений";
            }

            List<Room> pantriesWithoutBedrooms = apartment
                .GetRoomsByPrior(_priorities.Pantry)
                .Select(x => x.RevitRoom)
                .Where(x => !_pantriesWithBedroom.Contains(x.Id))
                .Where(x => ContourChecker.CheckArea(x, _settings.Accuracy, 1.5))
                .ToList();

            if(pantriesWithoutBedrooms.Any()) {
                return ContourChecker
                    .CheckAnyRoomSizes(pantriesWithoutBedrooms, _settings.Accuracy, 1000)
                    .GetDescription();
            } else {
                return "Нет";
            }
        }

        // УТП Постирочная.
        // В квартире есть минимум одно помещение с именем "Постирочная".
        public string CalculateLaundry(Apartment apartment) {
            bool hasLaundries = apartment
                .GetRoomsByPrior(_priorities.Laundry)
                .Any();

            if(hasLaundries) {
                return "Да";
            }
            return "Нет";
        }
    }
}
