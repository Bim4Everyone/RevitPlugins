using System;
using System.Collections.Generic;


using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models.Geometry;
internal class Polygon {
    public List<Line> Sides { get; set; }
    public XYZ Center { get; set; }
    public Guid Guid { get; set; }
}
