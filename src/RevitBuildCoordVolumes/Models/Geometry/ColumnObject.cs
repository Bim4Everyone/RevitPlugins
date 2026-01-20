using System;

namespace RevitBuildCoordVolumes.Models.Geometry;
internal class ColumnObject {
    public PolygonObject PolygonObject { get; set; }
    public string FloorName { get; set; }
    public double StartPosition { get; set; }
    public double FinishPosition { get; set; }
    public Guid StartSlabGuid { get; set; }
    public Guid FinishSlabGuid { get; set; }
    public bool IsSloped { get; set; }
}
