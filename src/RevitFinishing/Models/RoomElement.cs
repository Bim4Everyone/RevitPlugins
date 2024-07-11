using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;
using dosymep.Revit.Geometry;

namespace RevitFinishing.Models
{
    internal class RoomElement {
        private readonly Room _revitRoom;
        private readonly string _roomFinishingType;
        private readonly Document _document;
        private readonly ElementIntersectsSolidFilter _solidFilter;
        private readonly BoundingBoxIntersectsFilter _bbFilter;

        private readonly IReadOnlyCollection<Element> _allFinishing;

        private readonly IReadOnlyCollection<Element> _walls;
        private readonly IReadOnlyCollection<Element> _floors;
        private readonly IReadOnlyCollection<Element> _ceilings;
        private readonly IReadOnlyCollection<Element> _baseboards;

        public RoomElement(Room room, FinishingInProject finishingElements) {
            _revitRoom = room;
            _document = room.Document;
            _roomFinishingType = _revitRoom.GetParam("ОТД_Тип отделки").AsValueString();

            Solid roomSolid = room
                .ClosedShell
                .OfType<Solid>()
                .First();
            _solidFilter = new ElementIntersectsSolidFilter(roomSolid);

            BoundingBoxXYZ bbXYZ = roomSolid.GetBoundingBox();
            BoundingBoxXYZ transformedBB = bbXYZ.TransformBoundingBox(bbXYZ.Transform);
            Outline roomOutline = new Outline(transformedBB.Min, transformedBB.Max);
            _bbFilter = new BoundingBoxIntersectsFilter(roomOutline);

            _walls = GetElementsBySolidIntersection(finishingElements.Walls.Select(x => x.Id).ToList()).ToList();
            _floors = GetElementsBySolidIntersection(finishingElements.Floors.Select(x => x.Id).ToList()).ToList();
            _ceilings = GetElementsBySolidIntersection(finishingElements.Ceilings.Select(x => x.Id).ToList()).ToList();
            _baseboards = GetElementsBySolidIntersection(finishingElements.Baseboards.Select(x => x.Id).ToList()).ToList();

            _allFinishing = _walls
                .Concat(_floors)
                .Concat(_ceilings)
                .Concat(_baseboards)
                .ToList();
        }

        public Room RevitRoom => _revitRoom;
        public string RoomFinishingType => _roomFinishingType;

        public IReadOnlyCollection<Element> AllFinishing => _allFinishing;
        public IReadOnlyCollection<Element> Walls => _walls;
        public IReadOnlyCollection<Element> Floors => _floors;
        public IReadOnlyCollection<Element> Ceilings => _ceilings;
        public IReadOnlyCollection<Element> Baseboards => _baseboards;

        private IList<Element> GetElementsBySolidIntersection(ICollection<ElementId> elements) {
            if(elements.Count == 0) {
                return new List<Element>();
            }

            return new FilteredElementCollector(_document, elements)
               .WherePasses(_bbFilter)
               .WherePasses(_solidFilter)
               .ToElements();
        }
    }
}
