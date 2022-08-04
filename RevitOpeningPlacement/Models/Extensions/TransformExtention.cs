
using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class TransformExtention {
        public static Plane OfPlane(this Transform transform, Plane plane) {
            return Plane.CreateByOriginAndBasis(transform.OfPoint(plane.Origin), transform.OfVector(plane.XVec), transform.OfVector(plane.YVec));
        }
    }
}
