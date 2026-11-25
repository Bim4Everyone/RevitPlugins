using System;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Models;

/// <summary>Числовые границы расчёта. Допуски сравнения отделены от точности округления.</summary>
internal static class CalculationNumbers {
    /// <summary>Проверяет конечность исходного числа.</summary>
    public static bool IsFinite(double value) {
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }

    /// <summary>Округляет положительный размер до целого миллиметра.</summary>
    public static double? RoundDimension(double? value) {
        if(value is not double mm || !IsFinite(mm) || mm <= 0) {
            return null;
        }
        // После преобразования в мм значение ровно на половине шага может оказаться на несколько ULP ниже.
        double midpoint = Math.Floor(mm) + 0.5;
        if(Math.Abs(midpoint - mm) <= Math.Abs(mm) * SystemPluginConfig.CoordinateRoundoffFactor) {
            mm = midpoint;
        }
        double rounded = Math.Round(mm, MidpointRounding.AwayFromZero);
        return rounded > 0 ? rounded : null;
    }

    /// <summary>Округляет вниз, учитывая погрешность координат при проверке целого миллиметра.</summary>
    public static double FloorBearing(double limitedMm, double availableFeet, double ruleMm, double roundoffFeet) {
        double maximumFeet = availableFeet + roundoffFeet;
        double upper = Math.Ceiling(limitedMm);
        if(upper <= ruleMm && upper / SystemPluginConfig.MillimetersPerFoot <= maximumFeet) {
            return upper;
        }
        double lower = Math.Floor(limitedMm);
        // Перевод из футов может погрешностью округлить исходное значение ровно до целого миллиметра.
        return lower > 0 && lower / SystemPluginConfig.MillimetersPerFoot > maximumFeet ? lower - 1 : lower;
    }
}
