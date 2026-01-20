using System.Collections.Generic;


using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Geometry;
internal class PolygonObject {
    public List<Line> Sides { get; set; }
    public XYZ Center { get; set; }
}
