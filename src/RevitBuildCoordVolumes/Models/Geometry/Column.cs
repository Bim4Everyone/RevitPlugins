using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Geometry;
internal class Column {
    public Solid Solid { get; set; }
    public Polygon Polygon { get; set; }
    public double StartZ { get; set; }
    public double EndZ { get; set; }
    public string StartLevel { get; set; }
    public string EndLevel { get; set; }
    public string Floor { get; set; }
}
