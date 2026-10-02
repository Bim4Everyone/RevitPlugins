using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models;

internal class SlabElement {
    public Guid Guid { get; set; }
    public Floor Floor { get; set; }
    public Level Level { get; set; }
    public string LevelName { get; set; }
    public IList<CurveLoop> TopContour { get; set; }
    public IList<Face> TopFaces { get; set; }
    public bool IsSloped { get; set; }
    
    
    
    
    public CurveArrArray Profile { get; set; }
    public Transform Transform { get; set; }
    
}
