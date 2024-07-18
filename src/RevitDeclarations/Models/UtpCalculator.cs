using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;

using dosymep.WPF.Extensions;

using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Models {
    internal class UtpCalculator {
        private const int _minHighFlatHeight = 3400;
        private const int _minBalconyDepth = 1200;
        private const int _minPantryDepth = 1000;
        private const double _minPantryArea = 1.8;

        private readonly StringComparer _strComparer = StringComparer.OrdinalIgnoreCase;
        private readonly StringComparison _strComparison = StringComparison.OrdinalIgnoreCase;

        private readonly DeclarationProject _project;
        private readonly DeclarationSettings _settings;
        private readonly PrioritiesConfig _priorities;

        private readonly bool _hasNullAreas;
        private readonly bool _hasBannedNames;
        private readonly IEnumerable<ElementId> _roomsWithBathFamily;

        // Гардеробные с дверью в жилую комнату. Для УТП Гардеробная
        private IEnumerable<ElementId> _pantriesWithBedroom;
        // Мастер-спальни 
        private IEnumerable<ElementId> _masterBedrooms;
        // Санузлы, относящиеся к мастер спальням
        private IEnumerable<ElementId> _masterBathrooms;

        public UtpCalculator(DeclarationProject project, DeclarationSettings settings) {
            _project = project;
            _settings = settings;

            _hasNullAreas = CheckUtpNullAreas();
            _hasBannedNames = GetBannedUtpRoomNames().Any();
            _roomsWithBathFamily = GetRoomsWithBath();

            _priorities = _settings.PrioritiesConfig;
        }

        public void CalculateRoomsForUtp() {
            var bedroomBathroom = GetRoomsByDoors(_priorities.LivingRoom, _priorities.Bathroom);
            // Жилые комната, соединенные с санузлами
            List<ElementId> bedroomsWithBathroom = bedroomBathroom[_priorities.LivingRoom.Name];

            var pantryBedroom = GetRoomsByDoors(_priorities.Pantry, _priorities.LivingRoom);
            var pantryBathroom = GetRoomsByDoors(_priorities.Pantry, _priorities.Bathroom);
            // Гардеробные, соединенные с жилыми комнатами (не учитываются для УТП Гардеробная)
            _pantriesWithBedroom = pantryBedroom[_priorities.Pantry.Name];
            // Гардеробные, соединенные с санузлами
            List<ElementId> pantriesWithBathroom = pantryBathroom[_priorities.Pantry.Name];
            // Мастер-гардеробные (имеющие дверь в санузел) для определения мастер-спален
            List<ElementId> masterPantries = _pantriesWithBedroom.Intersect(pantriesWithBathroom).ToList();

            var bedroomPantry = GetRoomsByDoors(_priorities.LivingRoom, masterPantries);
            // Жилые комнаты, соединенные с мастер-гврдеробными
            var bedroomsWithPantryAndBathroom = bedroomPantry[_priorities.LivingRoom.Name];
            _masterBedrooms = bedroomsWithBathroom.Concat(bedroomsWithPantryAndBathroom).ToList();

            List<ElementId> bathroomsWithBedroom = bedroomBathroom[_priorities.Bathroom.Name];
            var bathroomPantry = GetRoomsByDoors(_priorities.Bathroom, masterPantries);
            // Санузлы, соединенные с мастер-гардеробными
            var bathroomsWithMasterPantry = bathroomPantry[_priorities.Bathroom.Name];
            // Санузлы, относящиеся к мастер-спальням для определение УТП Две ванны
            _masterBathrooms = bathroomsWithBedroom.Concat(bathroomsWithMasterPantry).ToList();
        }

        public IReadOnlyCollection<ErrorsListViewModel> CheckProjectForUtp() {
            ErrorsListViewModel areaErrorListVM = new ErrorsListViewModel() {
                Message = "Предупреждение",
                Description = "В проекте присутствуют помещения квартир с нулевыми системными площадями",
                DocumentName = _project.Document.Name
            };

            if(_hasNullAreas) {
                areaErrorListVM.Errors.Add(new ErrorElement(_project.Document.Name,
                    "В проекте присутствуют помещения квартир с нулевыми системными площадями. " +
                    "УТП \"Гардеробная\" и \"Увеличенная площадь балкона/лоджии\" не могут быть корректно рассчитаны."));
            }

            ErrorsListViewModel namesErrorListVM = new ErrorsListViewModel() {
                Message = "Предупреждение",
                Description = "В проекте присутствуют помещения c некорректными именами.",
                DocumentName = _project.Document.Name
            };

            if(_hasBannedNames) {
                string names = string.Join(",", GetBannedUtpRoomNames());

                namesErrorListVM.Errors.Add(new ErrorElement(_project.Document.Name,
                    $"В проекте присутствуют помещения квартир c некорректными именами: \"{names}\". " +
                    "УТП \"Мастер-спальня\" и \"Две ванные\" не могут быть корректно рассчитаны."));
            }

            ErrorsListViewModel bathesErrorListVM = new ErrorsListViewModel() {
                Message = "Предупреждение",
                Description = "В проекте отсутсвуют семейства ванн и душевых.",
                DocumentName = _project.Document.Name
            };

            if(_project.GetBathInstances().Count == 0) {
                bathesErrorListVM.Errors.Add(new ErrorElement(_project.Document.Name,
                    "В проекте отсутствуют экземпляры семейств с именами, включающими \"ванн\" или \"душев\". " +
                    "УТП \"Две ванные\" не может быть корректно рассчитано."));
            }

            return new List<ErrorsListViewModel>() { areaErrorListVM, namesErrorListVM, bathesErrorListVM };
        }

        // УТП Хайфлет.
        // Все помещения в квартире имеют высоту от 3400 мм.
        public string CalculateHighflat(Apartment apartment) {
            foreach(var room in apartment.Rooms) {
                double height = room.GetLengthParamValue(_settings.RoomsHeightParam, _settings.Accuracy);
                if(height < _minHighFlatHeight) {
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
        // с глубиной не менее 1200 мм.
        public string CalculateExtraBalconyArea(Apartment apartment) {
            if(_hasNullAreas) {
                return "Ошибка нулевых площадей";
            }

            List<RoomElement> summerRooms = new List<RoomElement>();
            summerRooms.AddRange(apartment.GetRoomsByPrior(_priorities.Balcony));
            summerRooms.AddRange(apartment.GetRoomsByPrior(_priorities.Loggia));
            List<Room> summerRevitRooms = summerRooms.Select(x => x.RevitRoom).ToList();

            if(summerRooms.Any()) {
                return ContourChecker
                    .CheckAnyRoomSizes(summerRevitRooms, _settings.Accuracy, 0, _minBalconyDepth)
                    .GetDescription();
            } else {
                return "Нет";
            }
        }

        // УТП Гардеробная.
        // Наличие в квартире минимум одного помещения с именем "Гардеробная".
        // Одна сторона помещения должна быть не менее 1000 мм, площадь от 1,8 м2.
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
                .Where(x => ContourChecker.CheckArea(x, _settings.Accuracy, _minPantryArea))
                .ToList();

            if(pantriesWithoutBedrooms.Any()) {
                return ContourChecker
                    .CheckAnyRoomSizes(pantriesWithoutBedrooms, _settings.Accuracy, _minPantryDepth)
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


        private bool CheckUtpNullAreas() {
            return _project
                .Rooms
                .Where(x => x.AreaRevit < 0.1)
                .Any();
        }

        private IEnumerable<string> GetBannedUtpRoomNames() {
            return _project
                .Rooms
                .Select(x => x.Name)
                .Where(x => _settings.BannedRoomNames.Contains(x, StringComparer.OrdinalIgnoreCase))
                .Distinct()
                .ToList();
        }

        private Dictionary<string, List<ElementId>> GetRoomsByDoors(RoomPriority priority1, RoomPriority priority2) {
            string name1 = priority1.Name;
            string name2 = priority2.Name;
            BuiltInParameter bltParam = BuiltInParameter.ROOM_NAME;
            IReadOnlyCollection<FamilyInstance> doors = _project.GetDoors();

            Dictionary<string, List<ElementId>> roomByNames = new Dictionary<string, List<ElementId>>(_strComparer) {
                [name1] = new List<ElementId>(),
                [name2] = new List<ElementId>()
            };

            foreach(var door in doors) {
                Room room1 = door.get_FromRoom(_project.Phase);
                Room room2 = door.get_ToRoom(_project.Phase);
                // У дверей может не быть помещения с одной стороны
                if(room1 != null && room2 != null) { 
                    string roomName1 = room1.get_Parameter(bltParam).AsString().ToLower();
                    string roomName2 = room2.get_Parameter(bltParam).AsString().ToLower();

                    if(string.Equals(roomName1, name1, _strComparison) && 
                       string.Equals(roomName2, name2, _strComparison)) {
                        roomByNames[name1].Add(room1.Id);
                        roomByNames[name2].Add(room2.Id);
                    }

                    if(string.Equals(roomName1, name2, _strComparison) && 
                       string.Equals(roomName2, name1, _strComparison)) {
                        roomByNames[name1].Add(room2.Id);
                        roomByNames[name2].Add(room1.Id);
                    }                
                }
            }

            return roomByNames;
        }

        private Dictionary<string, List<ElementId>> GetRoomsByDoors(RoomPriority priority1, List<ElementId> rooms) {
            string name1 = priority1.Name;
            BuiltInParameter bltParam = BuiltInParameter.ROOM_NAME;
            IReadOnlyCollection<FamilyInstance> doors = _project.GetDoors();

            Dictionary<string, List<ElementId>> roomByNames = new Dictionary<string, List<ElementId>>(_strComparer) {
                [name1] = new List<ElementId>(),
            };

            foreach(var door in doors) {
                Room room1 = door.get_FromRoom(_project.Phase);
                Room room2 = door.get_ToRoom(_project.Phase);
                // У дверей может не быть помещения с одной стороны
                if(room1 != null && room2 != null) {
                    string roomName1 = room1.get_Parameter(bltParam).AsString().ToLower();
                    string roomName2 = room2.get_Parameter(bltParam).AsString().ToLower();

                    if(string.Equals(roomName1, name1, _strComparison) && rooms.Contains(room2.Id)) {
                        roomByNames[name1].Add(room1.Id);
                    }

                    if(string.Equals(roomName2, name1, _strComparison) && rooms.Contains(room1.Id)) {
                        roomByNames[name1].Add(room2.Id);
                    }
                }
            }

            return roomByNames;
        }

        private IEnumerable<ElementId> GetRoomsWithBath() {
            return _project
                .GetBathInstances()
                .Select(x => x.get_Room(_project.Phase))
                .Distinct()
                .Select(x => x?.Id)
                .ToList();
        }
    }
}
