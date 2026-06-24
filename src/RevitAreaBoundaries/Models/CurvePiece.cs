using Autodesk.Revit.DB;

namespace RevitAreaBoundaries.Models;

public class CurvePiece {
    
    public Curve SourceCurve { get; set; }
    
    public Curve Piece { get; set; }
    
}
