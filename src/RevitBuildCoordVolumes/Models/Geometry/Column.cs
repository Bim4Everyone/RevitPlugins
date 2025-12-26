using System;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Geometry;
internal class Column {
    public Polygon Polygon { get; set; }
    public string LevelName { get; set; }
    public double StartPosition { get; set; }
    public double FinishPosition { get; set; }
    public Guid StartSlabGuid { get; set; }
    public Guid FinishSlabGuid { get; set; }
    public Solid Solid { get; set; }
    public bool IsSloped { get; set; }
}
