using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitCheckingLevels.Models {
    internal static class LevelExtensions {
        public static bool IsAlmostEqual(double left, double right,
            double eps = double.Epsilon) {
            return Math.Abs(left - right) < eps;
        }

#if REVIT_2020_OR_LESS

        public static double GetMeterElevation(Level level) =>
            UnitUtils.ConvertFromInternalUnits(level.Elevation, DisplayUnitType.DUT_METERS);

        public static double GetMillimeterElevation(Level level) =>
            UnitUtils.ConvertFromInternalUnits(level.Elevation, DisplayUnitType.DUT_METERS);

#else

        public static double GetMeterElevation(Level level) =>
            UnitUtils.ConvertFromInternalUnits(level.Elevation, UnitTypeId.Meters);

        public static double GetMillimeterElevation(Level level) =>
            UnitUtils.ConvertFromInternalUnits(level.Elevation, UnitTypeId.Millimeters);

#endif
    }
}
