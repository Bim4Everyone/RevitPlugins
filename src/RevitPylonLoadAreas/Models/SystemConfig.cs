using RevitPylonLoadAreas.Models.Geometry;

namespace RevitPylonLoadAreas.Models;

internal sealed class SystemConfig {
    public double FloorArcTessellationStep { get; set; } = 50.0 / 1000.0 * GeometryTolerance.FeetPerMeter;

    public double WallsTessellationStep { get; set; } = 300.0 / 1000.0 * GeometryTolerance.FeetPerMeter;

    public double OpeningMinArea { get; set; } = GeometryTolerance.SqFeetPerSqMeter;

    public double MinSiteDistance { get; set; } = 10.0 / 1000.0 * GeometryTolerance.FeetPerMeter;
}
