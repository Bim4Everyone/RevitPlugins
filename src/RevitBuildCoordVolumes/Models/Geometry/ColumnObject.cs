using System;

namespace RevitBuildCoordVolumes.Models.Geometry;

internal class ColumnObject {
    public PolygonObject PolygonObject { get; set; }
    public string LevelName { get; set; }
    public double StartPosition { get; set; }
    public double FinishPosition { get; set; }
    public SlabElement StartSlab { get; set; }
    public SlabElement FinishSlab { get; set; }
    public bool IsSloped { get; set; }
}
