using System;

using Autodesk.Revit.DB;

namespace RevitPunchingRebar.Models;

/// <summary>
/// Перевод длин между миллиметрами и внутренними единицами Revit.
/// </summary>
internal static class UnitUtilsHelper {
    // Допуск на погрешность вычислений с плавающей точкой при округлении, в долях шага
    private const double _roundingTolerance = 1e-9;

    /// <summary>
    /// Переводит миллиметры во внутренние единицы Revit.
    /// </summary>
    public static double FromMm(double mm) {
        return UnitUtils.ConvertToInternalUnits(mm, UnitTypeId.Millimeters);
    }

    /// <summary>
    /// Переводит внутренние единицы Revit в миллиметры.
    /// </summary>
    public static double ToMm(double value) {
        return UnitUtils.ConvertFromInternalUnits(value, UnitTypeId.Millimeters);
    }

    /// <summary>
    /// Округляет длину во внутренних единицах вверх до значения, кратного шагу в миллиметрах.
    /// </summary>
    /// <remarks>
    /// Значения, отличающиеся от кратного шагу только погрешностью вычислений, не округляются до следующего шага.
    /// </remarks>
    public static double CeilingToMm(double value, double stepMm) {
        return FromMm(Math.Ceiling(ToMm(value) / stepMm - _roundingTolerance) * stepMm);
    }
}
