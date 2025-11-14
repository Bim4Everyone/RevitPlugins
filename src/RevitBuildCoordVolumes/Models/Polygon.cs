using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models;
internal class Polygon {
    public List<Line> Sides { get; set; }
    public double LocationPointZ { get; set; }
}
