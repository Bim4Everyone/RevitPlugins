using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class CeilingAndFloorExtension {
        public static Face GetTopFace(this CeilingAndFloor ceilingAndFloor) {
            var faceRefs = HostObjectUtils.GetTopFaces(ceilingAndFloor);
            return (Face) ceilingAndFloor.GetGeometryObjectFromReference(faceRefs[0]);
        }

        public static Face GetBottomFace(this CeilingAndFloor ceilingAndFloor) {
            var faceRefs = HostObjectUtils.GetBottomFaces(ceilingAndFloor);
            return (Face) ceilingAndFloor.GetGeometryObjectFromReference(faceRefs[0]);
        }
    }
}
