using Autodesk.Revit.DB;

namespace RevitArchitecturalDocumentation.Models;
public class UnitUtilsHelper { 
    public static double ConvertFromInternalValue(double value) {
        // Перевод из внутренних единиц Revit

#if REVIT_2021_OR_GREATER
        var unitType = UnitTypeId.Millimeters;
#else
        DisplayUnitType unitType = DisplayUnitType.DUT_MILLIMETERS;
#endif
        return UnitUtils.ConvertFromInternalUnits(value, unitType);
    }


    public static double ConvertToInternalValue(double value) {
        // Перевод во внутренние единицы Revit

#if REVIT_2021_OR_GREATER
        var unitType = UnitTypeId.Millimeters;
#else
        DisplayUnitType unitType = DisplayUnitType.DUT_MILLIMETERS;
#endif
        return UnitUtils.ConvertToInternalUnits(value, unitType);
    }
}
