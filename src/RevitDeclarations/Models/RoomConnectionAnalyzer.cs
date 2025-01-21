using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using System.Runtime;

namespace RevitDeclarations.Models {
    internal class RoomConnectionAnalyzer {
        private readonly ApartmentsProject _project;
        private readonly ApartmentsSettings _settings;
        private readonly PrioritiesConfig _priorities;

        private readonly StringComparer _strComparer = StringComparer.OrdinalIgnoreCase;
        private readonly StringComparison _strComparison = StringComparison.OrdinalIgnoreCase;


        // Гардеробные с дверью в жилую комнату. Для УТП Гардеробная
        private IEnumerable<ElementId> _pantriesWithBedroom;
        // Мастер-спальни 
        private IEnumerable<ElementId> _masterBedrooms;
        // Санузлы, относящиеся к мастер спальням
        private IEnumerable<ElementId> _masterBathrooms;

        public RoomConnectionAnalyzer(ApartmentsProject project, ApartmentsSettings settings) {
            _project = project;
            _settings = settings;

            _priorities = _settings.PrioritiesConfig;
        }

        public bool CheckIsMasterBathroom(ElementId roomId) {
            return _masterBathrooms.Contains(roomId);
        }

        public bool CheckIsMasterBedroom(ElementId roomId) {
            return _masterBedrooms.Contains(roomId);
        }

        public bool CheckIsPantryWithBedroom(ElementId roomId) {
            return _pantriesWithBedroom.Contains(roomId);
        }

        public void FindConnections() {
            var bedroomBathroom = GetRoomsByDoors(_priorities.LivingRoom, _priorities.Bathroom);
            // Жилые комнаты, соединенные с санузлами
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
            // Жилые комнаты, соединенные с мастер-гардеробными
            var bedroomsWithPantryAndBathroom = bedroomPantry[_priorities.LivingRoom.Name];

            // Итоговый список мастер-спален
            _masterBedrooms = bedroomsWithBathroom
                .Concat(bedroomsWithPantryAndBathroom)
                .ToList();

            List<ElementId> bathroomsWithBedroom = bedroomBathroom[_priorities.Bathroom.Name];
            var bathroomPantry = GetRoomsByDoors(_priorities.Bathroom, masterPantries);
            // Санузлы, соединенные с мастер-гардеробными
            var bathroomsWithMasterPantry = bathroomPantry[_priorities.Bathroom.Name];

            // Санузлы, относящиеся к мастер-спальням для определение УТП Две ванны
            _masterBathrooms = bathroomsWithBedroom
                .Concat(bathroomsWithMasterPantry)
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

        //private Dictionary<string, List<ElementId>> GetRoomsBySeparators(RoomPriority priority1,
        //                                                                 RoomPriority priority2) {

        //}
    }
}
