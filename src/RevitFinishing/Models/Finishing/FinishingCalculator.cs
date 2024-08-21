using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitFinishing.Models.Finishing.FinishingElements;

namespace RevitFinishing.Models
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

            foreach(var room in _finishingRooms) {
                foreach(var finishingRevitElement in room.AllFinishing) {

                    allFinishings = AddToDictionary(allFinishings, room, finishingRevitElement);
                }
            }
            return allFinishings.Values.ToList();
        }

        private Dictionary<string, FinishingType> GroupRoomsByFinishingType() {
            return _finishingRooms
                .GroupBy(x => x.RoomFinishingType)
                .ToDictionary(x => x.Key, x => new FinishingType(x.ToList()));
        }

        private Dictionary<ElementId, FinishingElement> AddToDictionary  (
                                     Dictionary<ElementId, FinishingElement> allFinishings, 
                                     RoomElement room,
                                     Element finishingRevitElement) {
            ElementId finishingElementId = finishingRevitElement.Id;
            FinishingCategory categoryInstance = new FinishingCategory();

            if(allFinishings.TryGetValue(finishingElementId, out FinishingElement elementInDict)) {
                elementInDict.Rooms.Add(room);
            } else {
                if(categoryInstance.CheckCategory(finishingRevitElement, FinishingCategory.Walls)) {
                    var newFinishing = new FinishingWall(finishingRevitElement, this) {
                        Rooms = new List<RoomElement> { room }
                    };
                    allFinishings.Add(finishingElementId, newFinishing);
                };
                if(categoryInstance.CheckCategory(finishingRevitElement, FinishingCategory.Ceilings)) {
                    var newFinishing = new FinishingCeiling(finishingRevitElement, this) {
                        Rooms = new List<RoomElement> { room }
                    };
                    allFinishings.Add(finishingElementId, newFinishing);
                };
                if(categoryInstance.CheckCategory(finishingRevitElement, FinishingCategory.Floors)) {
                    var newFinishing = new FinishingFloor(finishingRevitElement, this) {
                        Rooms = new List<RoomElement> { room }
                    };
                    allFinishings.Add(finishingElementId, newFinishing);
                };
                if(categoryInstance.CheckCategory(finishingRevitElement, FinishingCategory.Baseboards)) {
                    var newFinishing = new FinishingBaseboard(finishingRevitElement, this) {
                        Rooms = new List<RoomElement> { room }
                    };
                    allFinishings.Add(finishingElementId, newFinishing);
                };

            }

            return allFinishings;
        }
    }
}
