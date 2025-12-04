using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models;

internal class SlabElement {
    public string Name { get; set; }
    public string LevelName { get; set; }
    public Floor Floor { get; set; }
    public Solid FloorSolid { get; set; }
    public List<XYZ> ExternalContourPoints { get; set; }
    public List<List<XYZ>> FullExternalContourPoints { get; set; }
    public List<Face> Faces { get; set; }
}
