using System;

using Autodesk.Revit.DB;

namespace RevitDeclarations.Models;
public static class ParamConverter {
    /// <summary>
    /// Конвертирует значение площади из внутренних единиц Revit в квадратные метры.
    /// </summary>
    public static double ConvertArea(double value, int accuracy) {
        double convertedValue = UnitUtils.ConvertFromInternalUnits(value, UnitTypeId.SquareMeters);
        return Math.Round(convertedValue, accuracy);
    }

    /// <summary>
    /// Конвертирует значение длины из внутренних единиц Revit в метры.
    /// </summary>
    public static double ConvertLength(double value, int accuracy) {
        double convertedValue = UnitUtils.ConvertFromInternalUnits(value, UnitTypeId.Meters);
        return Math.Round(convertedValue, accuracy);
    }

    /// <summary>
    /// Конвертирует значение длины из миллиметров во внутренние единицы Revit.
    /// </summary>
    public static double ConvertLengthToInternal(double value, int accuracy) {
        double convertedValue = UnitUtils.ConvertToInternalUnits(value, UnitTypeId.Millimeters);
        return Math.Round(convertedValue, accuracy);
    }
}
