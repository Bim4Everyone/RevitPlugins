using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitDeclarations.Models.Rooms;
using RevitDeclarations.Models.Rooms.Separators;

namespace RevitDeclarations.Models;
internal class RoomConnectionAnalyzer(ApartmentsProject project, PrioritiesConfig priorities) {
    private HashSet<ElementId> _masterBedrooms = [];
    private HashSet<ElementId> _masterBathrooms = [];
    private HashSet<ElementId> _pantriesWithBedroom = [];

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
        // Построение всех разделителей:
        // 1. Двери: помещение с одной стороны двери + помещение с другой стороны двери.
        // 2. Room Separation Lines: помещения, границы которых проходят по одной линии разделения и реально пересекаются.
        
        var separators = BuildSeparators();

        // Жилые помещения, соединенные с санузлами
        var bedroomBathroom = GetRoomsBySeparators(
            separators,
            priorities.LivingRoom,
            priorities.Bathroom);

        var bedroomsWithBathroom = bedroomBathroom[priorities.LivingRoom];

        // Жилые помещения, соединенные с гардеробными
        var pantryBedroom = GetRoomsBySeparators(
            separators,
            priorities.Pantry,
            priorities.LivingRoom);

        // Гардеробные, соединенные с санузлами
        var pantryBathroom = GetRoomsBySeparators(
            separators,
            priorities.Pantry,
            priorities.Bathroom);

        _pantriesWithBedroom = pantryBedroom[priorities.Pantry].ToHashSet();
        
        var pantriesWithBathroom = pantryBathroom[priorities.Pantry].ToHashSet();

        // Master pantry - гардеробные, соединенные с жилым помещением с санузлом
        var masterPantries = _pantriesWithBedroom
            .Intersect(pantriesWithBathroom)
            .ToHashSet();
        
        // Жилые помещения, соединенные с гардеробными (master pantry)
        var bedroomPantry = GetRoomsBySeparators(
            separators, priorities.LivingRoom, masterPantries);

        var bedroomsWithPantryAndBathroom = bedroomPantry[priorities.LivingRoom];
        
        // Master bedroom - жилые помещения, соединенные с гардеробными (master pantry), или жилые помещения, соединенные с санузлом
        _masterBedrooms = bedroomsWithBathroom
            .Concat(bedroomsWithPantryAndBathroom)
            .ToHashSet();
        
        // Санузлы, соединенные с жилыми помещениями
        var bathroomsWithBedroom = bedroomBathroom[priorities.Bathroom];

        // Санузлы, соединенные с жилыми помещениями
        var bathroomPantry = GetRoomsBySeparators(
            separators,
            priorities.Bathroom,
            masterPantries);

        var bathroomsWithMasterPantry = bathroomPantry[priorities.Bathroom];
        
        // Master bathroom - санузлы, соединенные с гардеробными (master pantry), или санузлы, соединенные с жилым помещением
        _masterBathrooms = bathroomsWithBedroom
            .Concat(bathroomsWithMasterPantry)
            .ToHashSet();
    }

    private List<RoomSeparator> BuildSeparators() {
        // 1. Двери: помещение с одной стороны двери + помещение с другой стороны двери.
        var doorSeparators = project
            .GetDoors()
            .Select(x => new FamInstanceRoomSeparator(project, x))
            .Where(x => x.CheckIsValid())
            .Cast<RoomSeparator>()
            .ToList();

        // 2. Room Separation Lines: помещения, границы которых проходят по одной линии разделения и реально пересекаются.
        var curveSeparators = BuildCurveSeparators();

        doorSeparators.AddRange(curveSeparators);
        
        return doorSeparators;
    }

    private List<RoomSeparator> BuildCurveSeparators() {
        var curves = project
            .GetCurveSeparators()
            .ToList();

        if (curves.Count == 0) {
            return [];
        }
        
        var boundaryCache = BuildBoundaryCache(curves);

        var result = new List<RoomSeparator>(curves.Count);

        foreach (var curve in curves) {
            var separator = new CurveRoomSeparator(curve, boundaryCache);
            
            if (separator.CheckIsValid()) {
                result.Add(separator);
            }
        }

        return result;
    }

    private RoomBoundaryCache BuildBoundaryCache(IReadOnlyList<CurveElement> curves) {
        var curveIds = curves
            .Select(x => x.Id)
            .ToHashSet();

        var cache = new RoomBoundaryCache();
        
        foreach (var room in project.Rooms) {
            var boundaries = room.GetBoundaries();

            foreach (var boundary in boundaries) {
                if (!curveIds.Contains(boundary.ElementId)) {
                    continue;
                }
                
                cache.Add(boundary.ElementId, room.RevitRoom, boundary);
            }
        }

        return cache;
    }

    private static Dictionary<RoomPriority, List<ElementId>> GetRoomsBySeparators(
        IReadOnlyList<RoomSeparator> separators,
        RoomPriority priority1,
        RoomPriority priority2) {
        
        var result = new Dictionary<RoomPriority, List<ElementId>> { [priority1] = [], [priority2] = [] };

        foreach (var separator in separators) {
            var room1 = separator.GetRoom(priority1);
            var room2 = separator.GetRoom(priority2);

            if (room1 == null || room2 == null) {
                continue;
            }

            result[priority1].Add(room1.Id);
            result[priority2].Add(room2.Id);
        }

        return result;
    }

    private static Dictionary<RoomPriority, List<ElementId>> GetRoomsBySeparators(
        IReadOnlyList<RoomSeparator> separators, RoomPriority priority, HashSet<ElementId> rooms) {
        
        var result = new Dictionary<RoomPriority, List<ElementId>> {
            [priority] = []
        };

        foreach (var separator in separators) {
            bool containsRoom = separator.Rooms
                .Any(x => rooms.Contains(x.Id));

            if (!containsRoom) {
                continue;
            }
           
            var room = separator.GetRoom(priority);

            if (room != null) {
                result[priority].Add(room.Id);
            }
        }

        return result;
    }
}
