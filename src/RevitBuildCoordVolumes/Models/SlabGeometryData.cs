using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Enums;

namespace RevitBuildCoordVolumes.Models;

public class SlabGeometryData {
    public List<CurveLoop> TopContour { get; set; }
    public List<CurveLoop> BottomContour { get; set; }
    public List<Face> TopFaces { get; set; }
    public double MaxPointZ { get; set; }
    public double MinPointZ { get; set; }
    public List<Solid> CheckingSolids { get; set; }
    public SlabType SlabType { get; set; }
}
