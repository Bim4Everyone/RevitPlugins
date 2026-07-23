using System.Collections.Generic;

using RevitAreaBoundaries.Models;
using RevitAreaBoundaries.Models.Enums;
using RevitAreaBoundaries.Models.Processors;

namespace RevitAreaBoundaries.Settings;

internal class AreaBoundarySettings {
    public IBoundaryDrawer BoundaryDrawer { get; set; }
    public double SectionHeightMm { get; set; }
    public List<RevitElement> Views { get; set; }
    public List<RevitElement> Types { get; set; }
    public string GroupParam { get; set; }
}
