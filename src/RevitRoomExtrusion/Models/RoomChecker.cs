using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Revit;


namespace RevitRoomExtrusion.Models {
    internal class RoomChecker {
        private readonly RevitRepository _revitRepository;

        public RoomChecker(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public bool CheckSelection() {
            return _revitRepository.GetSelectedRooms()
                .Count() > 0;
        }

        public bool CheckRooms() {
            return _revitRepository.GetSelectedRooms()
                .All(room => !CheckInvalidRoom(room));
        }

        public bool CheckInvalidRoom(Room room) {
            return room.IsNotEnclosed() || room.IsRedundant() || CheckIntersectBoundary(room);
        }

        public bool CheckIntersectBoundary(Room room) {
            SpatialElementBoundaryOptions options = new SpatialElementBoundaryOptions();
            return room.GetBoundarySegments(options)
                .Any(listSegment => CheckIntersectCurve(listSegment));
        }

        private bool CheckIntersectCurve(IList<BoundarySegment> segments) {
            List<Curve> curves = new List<Curve>();
            foreach(BoundarySegment segment in segments) {
                curves.Add(segment.GetCurve());
            }
            for(int i = 0; i < curves.Count; i++) {
                Curve curve1 = curves[i];
                for(int j = i + 1; j < curves.Count; j++) {
                    Curve curve2 = curves[j];
                    SetComparisonResult result = curve1.Intersect(curve2, out IntersectionResultArray results);
                    if(result == SetComparisonResult.Equal) {
                        return true;
                    }
                }
            }
            return false;
        }


    }
}
