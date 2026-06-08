using Autodesk.Revit.DB;

namespace RevitPylonLoadAreas.Models;

internal sealed class SystemConfig {
    public double FloorArcTessellationStep { get; set; } =
        UnitUtils.ConvertToInternalUnits(100.0, UnitTypeId.Millimeters);

    public double WallsTessellationStep { get; set; } =
        UnitUtils.ConvertToInternalUnits(300.0, UnitTypeId.Millimeters);

    public double OpeningMinArea { get; set; } =
        UnitUtils.ConvertToInternalUnits(1.0, UnitTypeId.SquareMeters);

    public double MinSiteDistance { get; set; } =
        UnitUtils.ConvertToInternalUnits(100.0, UnitTypeId.Millimeters);
}
