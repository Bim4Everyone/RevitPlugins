using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Enums;

namespace RevitBuildCoordVolumes.Models;

public class SlabGeometryData {
    public List<CurveLoop> Contour { get; set; }
    public List<Face> TopFaces { get; set; }
    public SlabType SlabType { get; set; }
}
