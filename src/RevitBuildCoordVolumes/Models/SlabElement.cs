using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models;

internal class SlabElement {

    public Floor Floor { get; set; }
    public string Name { get; set; }
    public IList<XYZ> ContourPoints { get; set; }
    public Document Document { get; set; }
}
