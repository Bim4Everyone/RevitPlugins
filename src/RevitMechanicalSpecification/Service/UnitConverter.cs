using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitMechanicalSpecification.Service {
    public static class UnitConverter {
        /// <summary>
        /// Из ревитовского дабла в метры
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static double DoubleToMeters(double number) {
            return Math.Round(UnitUtils.ConvertFromInternalUnits(number, UnitTypeId.Meters), 2);
        }

        /// <summary>
        /// Из ревитовского дабла в мм
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static double DoubleToMilimeters(double number) {
            return Math.Round(UnitUtils.ConvertFromInternalUnits(number, UnitTypeId.Millimeters), 2);
        }

        /// <summary>
        /// Из ревитовского дабла в метры квадратные
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static double DoubleToSquareMeters(double number) {
            return Math.Round(UnitUtils.ConvertFromInternalUnits(number, UnitTypeId.SquareMeters), 2);
        }

        /// <summary>
        /// Из ревитовского дабла в градусы
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static double DoubleToDegree(double number) {
            return Math.Round(UnitUtils.ConvertFromInternalUnits(number, UnitTypeId.Degrees), 2);
        }

        public static string DoubleToString(double number) {
            CultureInfo culture = new CultureInfo("ru-RU");
            return number.ToString(culture);
        }
    }
}
