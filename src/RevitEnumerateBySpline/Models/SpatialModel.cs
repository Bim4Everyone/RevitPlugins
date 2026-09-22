using Autodesk.Revit.DB;

namespace RevitEnumerateBySpline.Models;

internal class SpatialModel {
    public SpatialElement? SpatialElement { get; set; }
    public string? LevelName { get; set; }
}
