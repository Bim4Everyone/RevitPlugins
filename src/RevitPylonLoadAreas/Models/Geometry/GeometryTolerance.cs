namespace RevitPylonLoadAreas.Models.Geometry;

/// <summary>
/// Допуски геометрии плагина.
/// </summary>
internal static class GeometryTolerance {
    /// <summary>
    /// Общий допуск модели (в футах Revit). Используется для дедупликации точек,
    /// сравнения концов отрезков и стабилизации событий полигональной булевой операции.
    /// </summary>
    public const double Model = 1e-6;

    /// <summary>
    /// Допуск для сравнения площадей в фут^2. Площади ниже этого значения считаются нулевыми.
    /// </summary>
    public const double Area = 1e-9;

    /// <summary>
    /// Перевод метров в футы.
    /// </summary>
    public const double FeetPerMeter = 3.28083989501312;

    public static double MmToFeet(double mm) {
        return mm / 1000.0 * FeetPerMeter;
    }

    public static double SqMetersToSqFeet(double sqMeters) {
        return sqMeters * FeetPerMeter * FeetPerMeter;
    }

    public static double SqFeetToSqMeters(double sqFeet) {
        return sqFeet / (FeetPerMeter * FeetPerMeter);
    }
}
