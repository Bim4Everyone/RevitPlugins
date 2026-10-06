using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Enums;

namespace RevitBuildCoordVolumes.Models;

internal class SlabElement {
    public Guid Guid { get; set; }
    public SlabType SlabType { get; set; }
    public Floor Floor { get; set; }
    public string FloorName { get; set; }
    public Level Level { get; set; }
    public string LevelName { get; set; }
    public List<CurveLoop> TopContour { get; set; }
    public List<Face> TopFaces { get; set; }
}
