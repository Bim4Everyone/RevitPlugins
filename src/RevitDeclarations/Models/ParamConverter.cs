using System;

using Autodesk.Revit.DB;

namespace RevitDeclarations.Models
{
    public static class ParamConverter
    {
        public static double ConvertArea(double value, int accuracy) {
            double convertedValue = UnitUtils.ConvertFromInternalUnits(value, UnitTypeId.SquareMeters);
            return Math.Round(convertedValue, accuracy);
        }

        public static double ConvertLength(double value, int accuracy) {
            double convertedValue = UnitUtils.ConvertFromInternalUnits(value, UnitTypeId.Meters);
            return Math.Round(convertedValue, accuracy);
        }

        public static double ConvertLengthToInternal(double value, int accuracy) {
            double convertedValue = UnitUtils.ConvertToInternalUnits(value, UnitTypeId.Millimeters);
            return Math.Round(convertedValue, accuracy);
        }
    }
}
