using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitFinishing.Models.Finishing;
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

        IList<FinishingWall> walls = _rooms
            .SelectMany(x => x.AllFinishing.OfType<FinishingWall>()).ToList();
        IList<FinishingFloor> floors = _rooms
            .SelectMany(x => x.AllFinishing.OfType<FinishingFloor>()).ToList();
        IList<FinishingCeiling> ceilings = _rooms
            .SelectMany(x => x.AllFinishing.OfType<FinishingCeiling>()).ToList();
        IList<FinishingBaseboard> baseboards = _rooms
            .SelectMany(x => x.AllFinishing.OfType<FinishingBaseboard>()).ToList();

        _wallTypesByOrder = CalculateFinishingOrder(walls);
        _floorTypesByOrder = CalculateFinishingOrder(floors);
        _ceilingTypesByOrder = CalculateFinishingOrder(ceilings);
        _baseboardTypesByOrder = CalculateFinishingOrder(baseboards);
    }

    public IEnumerable<RoomElement> Rooms => _rooms;
    public int WallTypesNumber => _wallTypesByOrder.Count;
    public int FloorTypesNumber => _floorTypesByOrder.Count;
    public int CeilingTypesNumber => _ceilingTypesByOrder.Count;
    public int BaseboardTypesNumber => _baseboardTypesByOrder.Count;

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

    private List<string> CalculateFinishingOrder(IEnumerable<FinishingElement> finishingElements) {
        return finishingElements
            .Select(x => x.RevitElement.Name)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
    }
}
