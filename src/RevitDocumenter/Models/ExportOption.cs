using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models;

internal class ExportOption {
    public double MappingStepInFeet { get; set; }
    public Color ColorForAnchorLines { get; set; }
    public XYZ StartPointInRevit { get; set; }
    public XYZ EndPointInRevit { get; set; }
    public int StepCountX { get; set; }
    public int StepCountY { get; set; }
    public List<ElementId> AnchorLineIds { get; set; }
}
