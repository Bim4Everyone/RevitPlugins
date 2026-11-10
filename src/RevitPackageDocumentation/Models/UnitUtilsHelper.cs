using Autodesk.Revit.DB;

namespace RevitPackageDocumentation.Models;

/// <summary>
/// Перевод длин между миллиметрами и внутренними единицами Revit (футами)
/// </summary>
public static class UnitUtilsHelper {
    /// <summary>
    /// Переводит значение из внутренних единиц Revit в миллиметры
    /// </summary>
    public static double ConvertFromInternalValue(double value) {
#if REVIT_2021_OR_GREATER
        var unitType = UnitTypeId.Millimeters;
#else
        DisplayUnitType unitType = DisplayUnitType.DUT_MILLIMETERS;
#endif
        return UnitUtils.ConvertFromInternalUnits(value, unitType);
    }

    /// <summary>
    /// Переводит значение из миллиметров во внутренние единицы Revit
    /// </summary>
    public static double ConvertToInternalValue(double value) {
#if REVIT_2021_OR_GREATER
        var unitType = UnitTypeId.Millimeters;
#else
        DisplayUnitType unitType = DisplayUnitType.DUT_MILLIMETERS;
#endif
        return UnitUtils.ConvertToInternalUnits(value, unitType);
    }
}
