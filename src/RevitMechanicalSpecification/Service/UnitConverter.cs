using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitMechanicalSpecification.Service {
    public static class UnitConverter {

        public static double DoubleToMeters(double number) {
            return Math.Round(number * 304.8 / 1000, 2);
        }

        public static double DoubleToMilimeters(double number) {
            return Math.Round(number * 304.8, 2);
        }

        public static double DoubleToSquareMeters(double number) {
            return Math.Round(number * 0.092903, 2);
        }

        public static double DoubleToDegree(double number) {
            return Math.Round(number / 0.0175, 2);
        }
    }
}
