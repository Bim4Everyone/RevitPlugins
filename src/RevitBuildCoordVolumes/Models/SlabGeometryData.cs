using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models;

public class SlabGeometryData {
    public List<CurveLoop> Contour { get; set; }
    public List<Face> TopFaces { get; set; }
    public bool IsSloped { get; set; }
}
