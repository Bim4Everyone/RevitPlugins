
using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class PlaneExtention {
        internal static XYZ Evaluate(this Plane plane, UV uv) {
            return plane.Origin + uv.U * plane.XVec + uv.V * plane.YVec;
        }

        internal static XYZ ProjectPoint(this Plane plane, XYZ point) {
            plane.Project(point, out UV uvProjection, out double distance);
            return plane.Evaluate(uvProjection);
        }

        internal static XYZ ProjectVector(this Plane plane, XYZ vector) {
            plane.Project(vector, out UV uvProjection, out double distance);
            var end = plane.Evaluate(uvProjection);
            plane.Project(XYZ.Zero, out uvProjection, out distance);
            var start = plane.Evaluate(uvProjection);
            return end - start;
        }
    }
}
