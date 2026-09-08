using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitDeclarations.Models;
internal class CurveRoomSeparator : RoomSeparator {
    private readonly CurveElement _curve;

    public CurveRoomSeparator(ApartmentsProject project, CurveElement curve) {
        _curve = curve;

        foreach(var room in project.Rooms) {
            AddRoom(room);
        }
    }

    private void AddRoom(RoomElement room) {
        var roomSegments = room.GetBoundaries()
            .Where(x => x.ElementId == _curve.Id)
            .ToList();

        if(roomSegments.Count == 0) {
            return;
        }

        if(Rooms.Count == 0) {
            Rooms.Add(room.RevitRoom);
            return;
        }

        foreach(var addedRoom in Rooms) {
            var addedSegments = addedRoom
                .GetBoundarySegments(SpatialElementExtensions.DefaultBoundaryOptions)
                .SelectMany(x => x)
                .Where(x => x.ElementId == _curve.Id);

            if(!roomSegments.Any(x => addedSegments.Any(y => IsIntersectSegment(x, y)))) {
                continue;
            }

            Rooms.Add(room.RevitRoom);
            return;
        }
    }
    
    private static bool IsIntersectSegment(BoundarySegment first, BoundarySegment second) {
        var result = first.GetCurve().Intersect(second.GetCurve());

        return result 
            is SetComparisonResult.Overlap 
            or SetComparisonResult.Equal 
            or SetComparisonResult.Subset 
            or SetComparisonResult.Superset;
    }
}
