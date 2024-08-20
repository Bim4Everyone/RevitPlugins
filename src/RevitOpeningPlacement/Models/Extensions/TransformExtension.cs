
using System;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class TransformExtension {
        public static Plane OfPlane(this Transform transform, Plane plane) {
            return Plane.CreateByOriginAndBasis(transform.OfPoint(plane.Origin), transform.OfVector(plane.XVec), transform.OfVector(plane.YVec));
        }

        public static Transform GetRotationMatrixAroundZ(this Transform transform, double angle) {
            transform.BasisX = new XYZ(Math.Cos(angle), Math.Sin(angle), 0);
            transform.BasisY = new XYZ(-Math.Sin(angle), Math.Cos(angle), 0);
            transform.BasisZ = new XYZ(0, 0, 1);
            return transform;
        }
    }
}
