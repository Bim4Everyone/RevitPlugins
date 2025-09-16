using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Revit;


namespace RevitRoomExtrusion.Models;
internal class RoomChecker {
    private readonly RevitRepository _revitRepository;
    private readonly SpatialElementBoundaryOptions _options = new();

    public RoomChecker(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }

    // Метод проверки выделено ли что-то в проекте
    public bool CheckSelection() {
        return _revitRepository.GetSelectedRooms()
            .Count() > 0;
    }

    // Метод проверки неправильных помещений
    public bool CheckRooms() {
        return _revitRepository.GetSelectedRooms()
            .All(room => !CheckInvalidRoom(room));
    }

    // Метод проверки пересекающихся, избыточных и неокруженных помещений
    public bool CheckInvalidRoom(Room room) {
        return room.IsNotEnclosed()
            || room.IsRedundant()
            || CheckEqualBoundary(room);
    }

    // Метод выявления пересекающихся помещений
    public bool CheckEqualBoundary(Room room) {
        return _revitRepository.GetEqualCurves(
            room.GetBoundarySegments(_options)
                .SelectMany(segments => segments)
                .Select(segment => segment.GetCurve())
                .ToList())
            .Any();
    }
}
