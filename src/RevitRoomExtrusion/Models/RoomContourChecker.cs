using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;


namespace RevitRoomExtrusion.Models {
    internal class RoomContourChecker {
        public bool IsIntersectBoundary(Room room) {
            bool resultCheck = false;
            SpatialElementBoundaryOptions options = new SpatialElementBoundaryOptions();
            IList<IList<BoundarySegment>> boundaries = room.GetBoundarySegments(options);
            List<Curve> curves = new List<Curve>();

            foreach(BoundarySegment curve in boundaries[0]) {
                curves.Add(curve.GetCurve());
            }

            for(int i = 0; i < curves.Count; i++) {
                Curve curve1 = curves[i];
                for(int j = i + 1; j < curves.Count; j++) {
                    Curve curve2 = curves[j];

                    SetComparisonResult result = curve1.Intersect(curve2, out IntersectionResultArray results);

                    if(result == SetComparisonResult.Equal) {
                        resultCheck = true;
                        break;
                    }
                }
            }
            return resultCheck;
        }
    }
}
