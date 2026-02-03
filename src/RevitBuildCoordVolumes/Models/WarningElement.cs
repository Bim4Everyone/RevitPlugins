using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Geometry;

namespace RevitBuildCoordVolumes.Models;

internal abstract class WarningElement {
    public WarningType WarningType { get; set; }
    public SpatialObject SpatialObject { get; set; }
}
