using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models;

public class SlabGeometryData {
    public IList<CurveLoop> Contour { get; set; }
    public IList<Face> TopFaces { get; set; }
    public bool IsSloped { get; set; }
}
