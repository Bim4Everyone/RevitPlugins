namespace RevitPylonLoadAreas.Models.Geometry;

internal static class GeometryTolerance {
    public const double Model = 1e-6;
    public const double Area = 1e-9;
    public const double FeetPerMeter = 3.28083989501312;

    public static double MmToFeet(double mm) => mm / 1000.0 * FeetPerMeter;
    public static double SqMetersToSqFeet(double sqMeters) => sqMeters * FeetPerMeter * FeetPerMeter;
    public static double SqFeetToSqMeters(double sqFeet) => sqFeet / (FeetPerMeter * FeetPerMeter);
}
