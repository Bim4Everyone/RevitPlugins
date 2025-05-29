using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitFinishing.Models.Finishing
{
    /// <summary>
    /// Класс для группировки помещений по типу отделки.
    /// Тип отделки помещения определяется значением ключевого параметра помещения.
    /// </summary>
    internal class FinishingType {
        private readonly IEnumerable<RoomElement> _rooms;

        private readonly IList<string> _wallTypesByOrder;
        private readonly IList<string> _floorTypesByOrder;
        private readonly IList<string> _ceilingTypesByOrder;
        private readonly IList<string> _baseboardTypesByOrder;

        public FinishingType(IEnumerable<RoomElement> rooms) {
            _rooms = rooms;

            IList<Element> walls = _rooms.SelectMany(x => x.Walls).ToList();
            IList<Element> floors = _rooms.SelectMany(x => x.Floors).ToList();
            IList<Element> ceilings = _rooms.SelectMany(x => x.Ceilings).ToList();
            IList<Element> baseboards = _rooms.SelectMany(x => x.Baseboards).ToList();

            _wallTypesByOrder = CalculateFinishingOrder(walls);
            _floorTypesByOrder = CalculateFinishingOrder(floors);
            _ceilingTypesByOrder = CalculateFinishingOrder(ceilings);
            _baseboardTypesByOrder = CalculateFinishingOrder(baseboards);
        }

        public IEnumerable<RoomElement> Rooms => _rooms;


        public int GetWallOrder(string typeName) {
            return _wallTypesByOrder.IndexOf(typeName) + 1;
        }

        public int GetFloorOrder(string typeName) {
            return _floorTypesByOrder.IndexOf(typeName) + 1;
        }

        public int GetCeilingOrder(string typeName) {
            return _ceilingTypesByOrder.IndexOf(typeName) + 1;
        }

        public int GetBaseboardOrder(string typeName) {
            return _baseboardTypesByOrder.IndexOf(typeName) + 1;
        }

        private List<string> CalculateFinishingOrder(IEnumerable<Element> finishingElements) {
            return finishingElements
                .Select(x => x.Name)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }
    }
}
