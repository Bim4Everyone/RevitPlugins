using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitDeclarations.Models.Rooms.Separators;
internal class CurveRoomSeparator : RoomSeparator {
    public CurveRoomSeparator(CurveElement curve, RoomBoundaryCache boundaryCache) {
        if (!boundaryCache.TryGetRooms(curve.Id, out var roomBoundaries)) {
            return;
        }

        BuildRooms(roomBoundaries);
    }

    private void BuildRooms(Dictionary<ElementId, RoomBoundaryInfo> roomBoundaries) {
        var addedRooms = new List<RoomBoundaryInfo>();

        foreach (var roomBoundary in roomBoundaries.Values) {
            
            // Первое помещение, найденное на данной Room Separation Line, становится первым помещением separator
            if (addedRooms.Count == 0) {
                addedRooms.Add(roomBoundary);
                Rooms.Add(roomBoundary.Room);

                continue;
            }
            
            // Помещение добавляется только в том случае, если её сегменты
            // пересекаются с сегментами хотя бы одним уже добавленным помещением.
            bool isConnected = false;

            foreach (var addedRoom in addedRooms) {
                if (!AreConnected(roomBoundary.Segments, addedRoom.Segments)) {
                    continue;
                }

                isConnected = true;
                break;
            }

            if (!isConnected) {
                continue;
            }

            addedRooms.Add(roomBoundary);
            Rooms.Add(roomBoundary.Room);
        }
    }

    private static bool AreConnected(IReadOnlyList<BoundarySegment> firstSegments, IReadOnlyList<BoundarySegment> secondSegments) {
        foreach (var firstSegment in firstSegments) {
            var firstCurve = firstSegment.GetCurve();

            foreach (var secondSegment in secondSegments) {
                var result = firstCurve.Intersect(
                    secondSegment.GetCurve());

                if (result
                    is SetComparisonResult.Overlap
                    or SetComparisonResult.Equal
                    or SetComparisonResult.Subset
                    or SetComparisonResult.Superset) {
                    return true;
                }
            }
        }

        return false;
    }
}
