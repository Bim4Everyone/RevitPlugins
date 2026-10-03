using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Geometry;

internal class GeomObject {
    public List<GeometryObject> GeometryObjects { get; set; }
    public string LevelName { get; set; }
    public double Volume { get; set; }
}
