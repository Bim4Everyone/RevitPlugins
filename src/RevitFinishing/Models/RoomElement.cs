using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Revit.Geometry;

using RevitFinishing.Models.Finishing;

namespace RevitFinishing.Models;
/// <summary>
/// Класс для помещения в Revit, хранящей список всех элементов отделки, относящихся к этому помещению.
/// Получение отделки помещения производится через поиск пересечений Solid помещения с BoundingBox отделки.
/// </summary>
internal class RoomElement {
    private readonly Room _revitRoom;
    private readonly string _roomFinishingType;
    private readonly Document _document;
    private readonly ElementIntersectsSolidFilter _solidFilter;
    private readonly BoundingBoxIntersectsFilter _bbFilter;
    private readonly IReadOnlyCollection<FinishingElement> _allFinishing;

    public RoomElement(Room room, FinishingInProject finishingElements) {
        _revitRoom = room;
        _document = room.Document;
            _roomFinishingType = _revitRoom.GetParamValueString(ProjectParamsConfig.Instance.RoomFinishingType);

        Solid roomSolid = room
            .ClosedShell
            .OfType<Solid>()
            .First();
        _solidFilter = new ElementIntersectsSolidFilter(roomSolid);

        BoundingBoxXYZ bbXYZ = roomSolid.GetBoundingBox();
        BoundingBoxXYZ transformedBB = bbXYZ.TransformBoundingBox(bbXYZ.Transform);
        var roomOutline = new Outline(transformedBB.Min, transformedBB.Max);
        _bbFilter = new BoundingBoxIntersectsFilter(roomOutline);

        _allFinishing = GetElementsBySolidIntersection(finishingElements.AllFinishing.ToList());
    }

    public Room RevitRoom => _revitRoom;
    public string RoomFinishingType => _roomFinishingType;

    public IReadOnlyCollection<FinishingElement> AllFinishing => _allFinishing;

    private IReadOnlyCollection<FinishingElement> GetElementsBySolidIntersection(ICollection<FinishingElement> elements) {
        var filteredElements = !elements.Any()
            ? []
            : new FilteredElementCollector(_document, elements.Select(x => x.RevitElement.Id).ToList())
            .WherePasses(_bbFilter)
            .WherePasses(_solidFilter)
            .ToElementIds()
            .ToList();

        return elements.Where(x => filteredElements.Contains(x.RevitElement.Id)).ToList();
    }
}
