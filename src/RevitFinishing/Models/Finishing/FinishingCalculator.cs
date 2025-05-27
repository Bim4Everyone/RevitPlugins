using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;

namespace RevitFinishing.Models.Finishing
{
    /// <summary>
    /// Класс для расчетов отделки помещений в проекте Revit.
    /// </summary>
    internal class FinishingCalculator {
        private readonly List<Element> _revitRooms;
        private readonly FinishingInProject _revitFinishings;
        private readonly List<FinishingElement> _finishingElements;

        private readonly List<RoomElement> _finishingRooms;
        private readonly Dictionary<string, FinishingType> _roomsByFinishingType;

        public FinishingCalculator(IEnumerable<Element> rooms, FinishingInProject finishings) {
            _revitRooms = rooms.ToList();
            _revitFinishings = finishings;

            _finishingRooms = _revitRooms
                .OfType<Room>()
                .Select(x => new RoomElement(x, _revitFinishings))
                .ToList();
            _finishingElements = SetRoomsForFinishing();
            _roomsByFinishingType = GroupRoomsByFinishingType();
        }

        public List<FinishingElement> FinishingElements => _finishingElements;
        public Dictionary<string, FinishingType> RoomsByFinishingType => _roomsByFinishingType;

        /// <summary>
        /// Метод сопоставляет каждый элемент отделки с каждым помещением, 
        /// к которому этот элемент относится.
        /// </summary>
        /// <returns></returns>
        private List<FinishingElement> SetRoomsForFinishing() {
            Dictionary<ElementId, FinishingElement> allFinishings = new Dictionary<ElementId, FinishingElement>();
            string wallName = FinishingCategory.Walls.Name;
            string floorName = FinishingCategory.Floors.Name;
            string ceilingName = FinishingCategory.Ceilings.Name;
            string baseboarName = FinishingCategory.Baseboards.Name;

            foreach(var room in _finishingRooms) {
                foreach(var finishingRevitElement in room.Walls) {
                    allFinishings = UpdateDictionary(allFinishings, room, wallName, finishingRevitElement);
                }
                foreach(var finishingRevitElement in room.Baseboards) {
                    allFinishings = UpdateDictionary(allFinishings, room, baseboarName, finishingRevitElement);
                }
                foreach(var finishingRevitElement in room.Floors) {
                    allFinishings = UpdateDictionary(allFinishings, room, floorName, finishingRevitElement);
                }
                foreach(var finishingRevitElement in room.Ceilings) {
                    allFinishings = UpdateDictionary(allFinishings, room, ceilingName, finishingRevitElement);
                }
            }
            return allFinishings.Values.ToList();
        }

        private Dictionary<string, FinishingType> GroupRoomsByFinishingType() {
            return _finishingRooms
                .GroupBy(x => x.RoomFinishingType)
                .ToDictionary(x => x.Key, x => new FinishingType(x.ToList()));
        }

        private Dictionary<ElementId, FinishingElement> UpdateDictionary(
                                     Dictionary<ElementId, FinishingElement> allFinishings,
                                     RoomElement room,
                                     string finishingName,
                                     Element finishingRevitElement) {
            ElementId finishingId = finishingRevitElement.Id;

            if(allFinishings.TryGetValue(finishingId, out FinishingElement finishing)) {
                finishing.Rooms.Add(room);
            } else {
                var newFinishing = FinishingFactory.Create(finishingName, finishingRevitElement, this);
                newFinishing.Rooms = new List<RoomElement> { room };
                allFinishings.Add(finishingId, newFinishing);
            }

            return allFinishings;
        }
    }
}
