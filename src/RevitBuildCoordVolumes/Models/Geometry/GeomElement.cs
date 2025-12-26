using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Geometry;
internal class GeomElement {
    public List<GeometryObject> GeometryObjects { get; set; }
    public string FloorName { get; set; }
}
