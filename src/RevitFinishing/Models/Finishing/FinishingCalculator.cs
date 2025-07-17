using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace RevitFinishing.Models.Finishing;
/// <summary>
/// Класс для расчетов отделки помещений в проекте Revit.
/// </summary>
internal class FinishingCalculator {
    private readonly IList<FinishingElement> _finishingElements;
    private readonly IList<RoomElement> _finishingRooms;
    private readonly Dictionary<string, FinishingType> _roomsByFinishingType;

    public FinishingCalculator(IEnumerable<Element> rooms, FinishingInProject finishings) {
        _finishingRooms = rooms 
            .OfType<Room>()
            .Select(x => new RoomElement(x, finishings))
            .ToList();
        _finishingElements = GetUpdatedFinishingByRooms();
        _roomsByFinishingType = GroupRoomsByFinishingType();
    }

    public IList<FinishingElement> FinishingElements => _finishingElements;
    public Dictionary<string, FinishingType> RoomsByFinishingType => _roomsByFinishingType;

    /// <summary>
    /// Метод сопоставляет каждый элемент отделки с каждым помещением, 
    /// к которому этот элемент относится.
    /// </summary>
    /// <returns></returns>
    private IList<FinishingElement> GetUpdatedFinishingByRooms() {
        Dictionary<ElementId, FinishingElement> allFinishings = [];

        foreach(RoomElement room in _finishingRooms) {
            foreach(FinishingElement finishingRevitElement in room.AllFinishing) {
                allFinishings = UpdateDictionary(allFinishings, room, finishingRevitElement);
            }
        }
        return allFinishings.Values.ToList();
    }

    private Dictionary<string, FinishingType> GroupRoomsByFinishingType() {
        return _finishingRooms
            .GroupBy(x => x.RoomFinishingType)
            .ToDictionary(x => x.Key, x => new FinishingType(x));
    }

    private Dictionary<ElementId, FinishingElement> UpdateDictionary(Dictionary<ElementId, FinishingElement> finishings,
                                                                     RoomElement room,
                                                                     FinishingElement finishingRevitElement) {
        ElementId finishingId = finishingRevitElement.RevitElement.Id;

        if(finishings.TryGetValue(finishingId, out FinishingElement finishing)) {
            finishing.Rooms.Add(room);
        } else {
            finishingRevitElement.Rooms = [room];
            finishings.Add(finishingId, finishingRevitElement);
        }

        return finishings;
    }
}
