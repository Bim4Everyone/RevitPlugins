using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitLintelsManager.Models;

public class RevitWall {
    public WallType WallType { get; set; }
    public IEnumerable<Wall> Walls { get; set; }
}
