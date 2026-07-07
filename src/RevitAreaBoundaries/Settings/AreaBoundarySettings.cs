using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitAreaBoundaries.Models;
using RevitAreaBoundaries.Models.Enums;

namespace RevitAreaBoundaries.Settings;

public class AreaBoundarySettings {
    
    public List<View> TargetViews { get; set; }
    
    public AlgorithmType AlgorithmType { get; set; }
    
    public double SectionHeight { get; set; }
    
    public List<RevitElementView> Views { get; set; }
    
    public List<RevitElementType> Types { get; set; }
    
}
