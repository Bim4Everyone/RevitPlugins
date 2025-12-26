using System;

namespace RevitBuildCoordVolumes.Models.Geometry;
internal class SlabInfo {
    public double Position { get; set; }
    public string SlabLevelName { get; set; }
    public Guid SlabGuid { get; set; }
    public bool IsSloped { get; set; }
}
