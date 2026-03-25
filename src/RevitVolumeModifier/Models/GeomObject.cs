using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitVolumeModifier.Models;

internal class GeomObject {
    public List<GeometryObject> GeometryObjects { get; set; }
    public double Volume { get; set; }
}
