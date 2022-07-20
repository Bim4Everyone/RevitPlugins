using System;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class MepCurveExtention {
        public static bool IsHorizontal(this MEPCurve curve) {
            var line = (Line) ((LocationCurve) curve.Location).Curve;
            return Math.Abs(line.GetEndPoint(0).Z - line.GetEndPoint(1).Z) < 0.0001;
        }
    }
}
