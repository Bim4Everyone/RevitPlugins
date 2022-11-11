
using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class PlaneExtension {
        public static XYZ Evaluate(this Plane plane, UV uv) {
            return plane.Origin + uv.U * plane.XVec + uv.V * plane.YVec;
        }

        public static XYZ ProjectPoint(this Plane plane, XYZ point) {
            plane.Project(point, out UV uvProjection, out double distance);
            return plane.Evaluate(uvProjection);
        }

        public static XYZ ProjectVector(this Plane plane, XYZ vector) {
            plane.Project(vector, out UV uvProjection, out double distance);
            var end = plane.Evaluate(uvProjection);
            plane.Project(XYZ.Zero, out uvProjection, out distance);
            var start = plane.Evaluate(uvProjection);
            return end - start;
        }

        public static double GetAngleOnPlaneToYAxis(this Plane plane, XYZ xyz) {
            return plane.YVec.AngleOnPlaneTo(xyz, plane.Normal);
        }
    }
}
