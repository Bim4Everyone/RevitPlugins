using System;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class MepCurveExtention {
        public static bool IsHorizontal(this MEPCurve curve) {
            var line = (Line) ((LocationCurve) curve.Location).Curve;
            return Math.Abs(line.GetEndPoint(0).Z - line.GetEndPoint(1).Z) < 0.0001;
        }

        public static bool IsPerpendicular(this MEPCurve curve, Wall wall) {
            var line = (Line) ((LocationCurve) curve.Location).Curve;
            return Math.Abs(line.Direction.AngleTo(wall.Orientation)) < 0.0001
                || Math.Abs(line.Direction.AngleTo(wall.Orientation) - Math.PI) < 0.0001;
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
