using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitOpeningPlacement.Models.OpeningPlacement;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class MepCurveExtention {
        public static Line GetLine(this MEPCurve curve) {
            return (Line) ((LocationCurve) curve.Location).Curve;
        }

        public static bool IsHorizontal(this MEPCurve curve) {
            return curve.GetLine().IsHorizontal();
        }

        public static bool IsVertical(this MEPCurve curve) {
            return curve.GetLine().IsVertical();
        }

        public static bool IsPerpendicular(this MEPCurve curve, Wall wall) {
            return curve.GetLine().IsPerpendicular(wall.GetLine())
                && curve.GetLine().IsPerpendicular(XYZ.BasisZ);
        }

        public static bool IsParallel(this MEPCurve curve, Wall wall) {
            return curve.GetLine().IsParallel(wall.GetLine());
        }

        public static double GetDiameter(this MEPCurve curve) {
            return curve.GetBuiltInSize(RevitRepository.MepCurveDiameters);
        }

        public static double GetHeight(this MEPCurve curve) {
            return curve.GetBuiltInSize(RevitRepository.MepCurveHeights);
        }

        public static double GetWidth(this MEPCurve curve) {
            return curve.GetBuiltInSize(RevitRepository.MepCurveWidths);
        }

        private static double GetBuiltInSize(this MEPCurve curve, IEnumerable<BuiltInParameter> parameters) {
            foreach(var param in parameters) {
                if(curve.IsExistsParam(param)) {
                    return curve.GetParamValueOrDefault<double>(param);
                }
            }
            return 0;
        }
    }
}
