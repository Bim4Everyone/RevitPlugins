using Autodesk.Revit.DB;

namespace RevitEnumerateBySpline.Models;

/// <summary>Помещение и сведения о его уровне.</summary>
internal class SpatialModel {
    public SpatialElement? SpatialElement { get; set; }
    public string? LevelName { get; set; }
}
