using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitMechanicalSpecification.Service {
    public static class UnitConverter {

        public static double DoubleToMeters(double number) {
            
            return Math.Round(UnitUtils.ConvertToInternalUnits(number, UnitTypeId.Meters), 2);
        }

        public static double DoubleToMilimeters(double number) {
            return Math.Round(UnitUtils.ConvertToInternalUnits(number, UnitTypeId.Millimeters), 2);
        }

        public static double DoubleToSquareMeters(double number) {
            return Math.Round(UnitUtils.ConvertToInternalUnits(number, UnitTypeId.SquareMeters), 2);
        }
        public static double DoubleToDegree(double number) {
            return Math.Round(UnitUtils.ConvertToInternalUnits(number, UnitTypeId.Degrees), 2);
        }
    }
}
