using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitAreaBoundaries.Models;
using RevitAreaBoundaries.Models.Enums;

namespace RevitAreaBoundaries.Settings;

public class AreaBoundarySettings {
    public AlgorithmType AlgorithmType { get; set; }
    public double SectionHeightMm { get; set; }
    public List<RevitElement> Views { get; set; }
    public List<RevitElement> Types { get; set; }
    public string GroupParam { get; set; }
}
