using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitDeclarations.Models;
internal class RoomConnectionAnalyzer {
    private readonly ApartmentsProject _project;
    private readonly PrioritiesConfig _priorities;

    // Мастер-спальни 
    private IEnumerable<ElementId> _masterBedrooms = [];
    // Санузлы, относящиеся к мастер спальням
    private IEnumerable<ElementId> _masterBathrooms = [];
    // Гардеробные с дверью (разделителем) в жилую комнату. Для УТП Гардеробная
    private IEnumerable<ElementId> _pantriesWithBedroom = [];

    public RoomConnectionAnalyzer(ApartmentsProject project, PrioritiesConfig priorities) {
        _project = project;
        _priorities = priorities;
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
        IEnumerable<RoomSeparator> doorSeparators = _project
            .GetDoors()
            .Select(x => new FamInstanceRoomSeparator(_project, x));

        IEnumerable<RoomSeparator> curveSeparators = _project
            .GetCurveSeparators()
            .Select(x => new CurveRoomSeparator(_project, x));

        var separators = doorSeparators
            .Concat(curveSeparators)
            .Where(x => x.CheckIsValid());

        // Словарь с жилыми комнатами, соединенными с СУ и этими СУ
        var bedroomBathroom = GetRoomsBySeparators(separators, _priorities.LivingRoom, _priorities.Bathroom);
        // Жилые комнаты, соединенные с санузлами
        var bedroomsWithBathroom = bedroomBathroom[_priorities.LivingRoom];

        var pantryBedroom = GetRoomsBySeparators(separators, _priorities.Pantry, _priorities.LivingRoom);
        var pantryBathroom = GetRoomsBySeparators(separators, _priorities.Pantry, _priorities.Bathroom);
        // Гардеробные, соединенные с жилыми комнатами (не учитываются для УТП Гардеробная)
        _pantriesWithBedroom = pantryBedroom[_priorities.Pantry];
        // Гардеробные, соединенные с санузлами
        var pantriesWithBathroom = pantryBathroom[_priorities.Pantry];

        // Мастер-гардеробные (имеющие дверь в санузел) для определения мастер-спален
        var masterPantries = _pantriesWithBedroom.Intersect(pantriesWithBathroom).ToList();

        var bedroomPantry = GetRoomsBySeparators(separators, _priorities.LivingRoom, masterPantries);
        // Жилые комнаты, соединенные с мастер-гардеробными
        var bedroomsWithPantryAndBathroom = bedroomPantry[_priorities.LivingRoom];

        // Итоговый список мастер-спален
        _masterBedrooms = bedroomsWithBathroom
            .Concat(bedroomsWithPantryAndBathroom)
            .ToList();

        var bathroomsWithBedroom = bedroomBathroom[_priorities.Bathroom];
        var bathroomPantry = GetRoomsBySeparators(separators, _priorities.Bathroom, masterPantries);
        // Санузлы, соединенные с мастер-гардеробными
        var bathroomsWithMasterPantry = bathroomPantry[_priorities.Bathroom];

        // Санузлы, относящиеся к мастер-спальням для определение УТП Две ванны
        _masterBathrooms = bathroomsWithBedroom
            .Concat(bathroomsWithMasterPantry)
            .ToList();
    }

    private Dictionary<RoomPriority, List<ElementId>> GetRoomsBySeparators(IEnumerable<RoomSeparator> separators,
                                                                           RoomPriority priority1,
                                                                           RoomPriority priority2) {
        var roomByNames = new Dictionary<RoomPriority, List<ElementId>>() {
            [priority1] = [],
            [priority2] = []
        };

        foreach(var separator in separators) {
            var room1 = separator.GetRoom(priority1);
            var room2 = separator.GetRoom(priority2);
            if(room1 != null && room2 != null) {
                roomByNames[priority1].Add(room1.Id);
                roomByNames[priority2].Add(room2.Id);
            }
        }

        return roomByNames;
    }

    private Dictionary<RoomPriority, List<ElementId>> GetRoomsBySeparators(IEnumerable<RoomSeparator> separators,
                                                                           RoomPriority priority,
                                                                           List<ElementId> rooms) {
        var roomByNames = new Dictionary<RoomPriority, List<ElementId>>() {
            [priority] = []
        };

        foreach(var separator in separators) {
            if(separator.Rooms.Where(x => rooms.Contains(x.Id)).Any()) {
                var room = separator.GetRoom(priority);
                if(room != null) {
                    roomByNames[priority].Add(room.Id);
                }
            }
        }

        return roomByNames;
    }
}
