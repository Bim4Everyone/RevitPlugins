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

        private readonly BuiltInParameter _bltNameParam = BuiltInParameter.ROOM_NAME;


        // Мастер-спальни 
        private IEnumerable<ElementId> _masterBedrooms;
        // Санузлы, относящиеся к мастер спальням
        private IEnumerable<ElementId> _masterBathrooms;
        // Гардеробные с дверью (разделителем) в жилую комнату. Для УТП Гардеробная
        private IEnumerable<ElementId> _pantriesWithBedroom;

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
            // Словарь с жилыми комнатами, соединенными с СУ и этими СУ
            var bedroomBathroom = GetRoomsByDoors(_priorities.LivingRoom, _priorities.Bathroom);
            // Жилые комнаты, соединенные с санузлами
            List<ElementId> bedroomsWithBathroom = bedroomBathroom[_priorities.LivingRoom];

            var pantryBedroom = GetRoomsByDoors(_priorities.Pantry, _priorities.LivingRoom);
            var pantryBathroom = GetRoomsByDoors(_priorities.Pantry, _priorities.Bathroom);
            // Гардеробные, соединенные с жилыми комнатами (не учитываются для УТП Гардеробная)
            _pantriesWithBedroom = pantryBedroom[_priorities.Pantry];
            // Гардеробные, соединенные с санузлами
            List<ElementId> pantriesWithBathroom = pantryBathroom[_priorities.Pantry];
            // Мастер-гардеробные (имеющие дверь в санузел) для определения мастер-спален
            List<ElementId> masterPantries = _pantriesWithBedroom.Intersect(pantriesWithBathroom).ToList();

            var bedroomPantry = GetRoomsByDoors(_priorities.LivingRoom, masterPantries);
            // Жилые комнаты, соединенные с мастер-гардеробными
            var bedroomsWithPantryAndBathroom = bedroomPantry[_priorities.LivingRoom];

            // Итоговый список мастер-спален
            _masterBedrooms = bedroomsWithBathroom
                .Concat(bedroomsWithPantryAndBathroom)
                .ToList();

            List<ElementId> bathroomsWithBedroom = bedroomBathroom[_priorities.Bathroom];
            var bathroomPantry = GetRoomsByDoors(_priorities.Bathroom, masterPantries);
            // Санузлы, соединенные с мастер-гардеробными
            var bathroomsWithMasterPantry = bathroomPantry[_priorities.Bathroom];

            // Санузлы, относящиеся к мастер-спальням для определение УТП Две ванны
            _masterBathrooms = bathroomsWithBedroom
                .Concat(bathroomsWithMasterPantry)
                .ToList();
        }

        private Dictionary<RoomPriority, List<ElementId>> GetRoomsByDoors(RoomPriority priority1, RoomPriority priority2) {
            IReadOnlyCollection<FamilyInstance> doors = _project.GetDoors();

            var roomByNames = new Dictionary<RoomPriority, List<ElementId>>() {
                [priority1] = new List<ElementId>(),
                [priority2] = new List<ElementId>()
            };

            foreach(var door in doors) {
                Room room1 = door.get_FromRoom(_project.Phase);
                Room room2 = door.get_ToRoom(_project.Phase);
                // У дверей может не быть помещения с одной стороны
                if(room1 != null && room2 != null) {
                    string roomName1 = room1.get_Parameter(_bltNameParam).AsString();
                    string roomName2 = room2.get_Parameter(_bltNameParam).AsString();

                    if(priority1.CheckName(roomName1) && priority2.CheckName(roomName2)) {
                        roomByNames[priority1].Add(room1.Id);
                        roomByNames[priority2].Add(room2.Id);
                    }

                    if(priority1.CheckName(roomName2) && priority2.CheckName(roomName1)) {
                        roomByNames[priority1].Add(room2.Id);
                        roomByNames[priority2].Add(room1.Id);
                    }
                }
            }

            return roomByNames;
        }

        private Dictionary<RoomPriority, List<ElementId>> GetRoomsByDoors(RoomPriority priority, List<ElementId> rooms) {
            IReadOnlyCollection<FamilyInstance> doors = _project.GetDoors();

            var roomByNames = new Dictionary<RoomPriority, List<ElementId>>() {
                [priority] = new List<ElementId>()
            };

            foreach(var door in doors) {
                Room room1 = door.get_FromRoom(_project.Phase);
                Room room2 = door.get_ToRoom(_project.Phase);
                // У дверей может не быть помещения с одной стороны
                if(room1 != null && room2 != null) {
                    string roomName1 = room1.get_Parameter(_bltNameParam).AsString();
                    string roomName2 = room2.get_Parameter(_bltNameParam).AsString();

                    if(priority.CheckName(roomName1) && rooms.Contains(room2.Id)) {
                        roomByNames[priority].Add(room1.Id);
                    }

                    if(priority.CheckName(roomName2) && rooms.Contains(room1.Id)) {
                        roomByNames[priority].Add(room2.Id);
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
