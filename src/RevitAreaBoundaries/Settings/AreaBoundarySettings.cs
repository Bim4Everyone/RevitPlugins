using System.Collections.Generic;

using RevitAreaBoundaries.Models;
using RevitAreaBoundaries.Models.Enums;

namespace RevitAreaBoundaries.Settings;

internal class AreaBoundarySettings {
    public AlgorithmType AlgorithmType { get; set; }
    public double SectionHeightMm { get; set; }
    public List<RevitElement> Views { get; set; }
    public List<RevitElement> Types { get; set; }
    public string GroupParam { get; set; }
}
