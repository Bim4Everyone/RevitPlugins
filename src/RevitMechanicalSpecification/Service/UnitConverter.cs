using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitMechanicalSpecification.Service {
    internal class UnitConverter {

        public double DoubleToMeters(double number) {
            return Math.Round(number * 304.8 / 1000, 2);
        }

        public double DoubleToMilimeters(double number) {
            return Math.Round(number * 304.8, 2);
        }

        public double DoubleToSquareMeters(double number) {
            return Math.Round(number * 0.092903, 2);
        }

        public double DoubleToDegree(double number) {
            return Math.Round(number / 0.0175, 2);
        }


    }
}
