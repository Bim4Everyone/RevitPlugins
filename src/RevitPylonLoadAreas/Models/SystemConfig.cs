using RevitPylonLoadAreas.Models.Geometry;

namespace RevitPylonLoadAreas.Models;

internal sealed class SystemConfig {
    public double FloorArcTessellationStep { get; set; } = GeometryTolerance.MmToFeet(50.0);

    public double WallsTessellationStep { get; set; } = GeometryTolerance.MmToFeet(300.0);

    public double OpeningMinArea { get; set; } = GeometryTolerance.SqMetersToSqFeet(1.0);

    public double MinSiteDistance { get; set; } = GeometryTolerance.MmToFeet(10.0);
}
