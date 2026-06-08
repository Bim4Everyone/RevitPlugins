namespace RevitPylonLoadAreas.Models.Geometry;

internal static class GeometryTolerance {
    public const double Model = 1e-6;
    public const double Area = 1e-9;
    public const double FeetPerMeter = 3.28083989501312;
    public const double SqFeetPerSqMeter = FeetPerMeter * FeetPerMeter;
}
