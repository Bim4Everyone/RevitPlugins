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
    private readonly IEnumerable<RoomElement> _finishingRooms;
    private readonly Dictionary<string, FinishingType> _roomsByFinishingType;

    public FinishingCalculator(IEnumerable<Element> rooms, FinishingInProject finishings) {
        _finishingRooms = rooms 
            .OfType<Room>()
            .Select(x => new RoomElement(x, finishings));
        _finishingElements = SetRoomsForFinishing();
        _roomsByFinishingType = GroupRoomsByFinishingType();
    }

    public IList<FinishingElement> FinishingElements => _finishingElements;
    public Dictionary<string, FinishingType> RoomsByFinishingType => _roomsByFinishingType;

    /// <summary>
    /// Метод сопоставляет каждый элемент отделки с каждым помещением, 
    /// к которому этот элемент относится.
    /// </summary>
    /// <returns></returns>
    private IList<FinishingElement> SetRoomsForFinishing() {
        Dictionary<ElementId, FinishingElement> allFinishings = [];
        string wallName = FinishingCategory.Walls.Name;
        string floorName = FinishingCategory.Floors.Name;
        string ceilingName = FinishingCategory.Ceilings.Name;
        string baseboarName = FinishingCategory.Baseboards.Name;

        foreach(RoomElement room in _finishingRooms) {
            foreach(Element finishingRevitElement in room.Walls) {
                allFinishings = UpdateDictionary(allFinishings, room, wallName, finishingRevitElement);
            }
            foreach(Element finishingRevitElement in room.Baseboards) {
                allFinishings = UpdateDictionary(allFinishings, room, baseboarName, finishingRevitElement);
            }
            foreach(Element finishingRevitElement in room.Floors) {
                allFinishings = UpdateDictionary(allFinishings, room, floorName, finishingRevitElement);
            }
            foreach(Element finishingRevitElement in room.Ceilings) {
                allFinishings = UpdateDictionary(allFinishings, room, ceilingName, finishingRevitElement);
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
                                                                     string finishingName,
                                                                     Element finishingRevitElement) {
        ElementId finishingId = finishingRevitElement.Id;

        if(finishings.TryGetValue(finishingId, out FinishingElement finishing)) {
            finishing.Rooms.Add(room);
        } else {
            FinishingElement newFinishing = FinishingFactory.Create(finishingName, finishingRevitElement, this);
            newFinishing.Rooms = [room];
            finishings.Add(finishingId, newFinishing);
        }

        return finishings;
    }
}
