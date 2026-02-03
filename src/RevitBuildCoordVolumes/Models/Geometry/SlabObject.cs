using System;

namespace RevitBuildCoordVolumes.Models.Geometry;

internal class SlabObject {
    public double Position { get; set; }
    public string FloorName { get; set; }
    public Guid SlabGuid { get; set; }
    public bool IsSloped { get; set; }
}
