using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class MepCurveExtention {
        public static bool IsHorizontal(this MEPCurve curve) {
            var line = (Line) ((LocationCurve) curve.Location).Curve;
            return line.IsHorizontal();
        }

        public static bool IsVertical(this MEPCurve curve) {
            var line = (Line) ((LocationCurve) curve.Location).Curve;
            return line.IsVertical();
        }

        public static bool IsPerpendicular(this MEPCurve curve, Wall wall) {
            var mepLine = (Line) ((LocationCurve) curve.Location).Curve;
            var wallLine = (Line) ((LocationCurve) wall.Location).Curve;

            return mepLine.IsPerpendicular(wallLine);
        }

        public static bool IsParallel(this MEPCurve curve, Wall wall) {
            var mepLine = (Line) ((LocationCurve) curve.Location).Curve;
            var wallLine = (Line) ((LocationCurve) wall.Location).Curve;

            return mepLine.IsParallel(wallLine);
        }

        public static XYZ GetIntersectionWithFace(this MEPCurve curve, Face face) {
            var line = (Line) ((LocationCurve) curve.Location).Curve;
            return line.GetIntersectionWithFaceFromEquation(face);
        }

        public static double GetDiameter(this MEPCurve curve) {
            foreach(var param in RevitRepository.MepCurveDiameters) {
                if(curve.IsExistsParam(param)) {
                    return curve.GetParamValueOrDefault<double>(param);
                }
            }
            return 0;
        }
    }
}
