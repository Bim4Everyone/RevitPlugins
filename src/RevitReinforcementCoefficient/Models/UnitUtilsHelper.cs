using Autodesk.Revit.DB;

namespace RevitReinforcementCoefficient.Models {
    public class UnitUtilsHelper {
        public static double ConvertFromInternalValue(double value) {
            // Перевод из внутренних единиц Revit длины
#if REVIT_2021_OR_GREATER
            ForgeTypeId unitType = UnitTypeId.Millimeters;
#else
            DisplayUnitType unitType = DisplayUnitType.DUT_MILLIMETERS;
#endif
            return UnitUtils.ConvertFromInternalUnits(value, unitType);
        }

        public static double ConvertVolumeFromInternalValue(double value) {
            // Перевод из внутренних единиц Revit объема
#if REVIT_2021_OR_GREATER
            ForgeTypeId unitType = UnitTypeId.CubicMeters;
#else
            DisplayUnitType unitType = DisplayUnitType.DUT_MILLIMETERS;
#endif
            return UnitUtils.ConvertFromInternalUnits(value, unitType);
        }

        public static double ConvertToInternalValue(double value) {
            // Перевод во внутренние единицы Revit
#if REVIT_2021_OR_GREATER
            ForgeTypeId unitType = UnitTypeId.Millimeters;
#else
            DisplayUnitType unitType = DisplayUnitType.DUT_MILLIMETERS;
#endif
            return UnitUtils.ConvertToInternalUnits(value, unitType);
        }
    }
}
