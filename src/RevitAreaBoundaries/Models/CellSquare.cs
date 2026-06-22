using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitAreaBoundaries.Models;

public class CellSquare
{
    public XYZ BottomLeft { get; set; }
    public XYZ BottomRight { get; set; }
    public XYZ TopRight { get; set; }
    public XYZ TopLeft { get; set; }
    
    public CellVertexType BLType { get; set; }
    public CellVertexType BRType { get; set; }
    public CellVertexType TRType { get; set; }
    public CellVertexType TLType { get; set; }
    
    public List<Curve> Curves { get; set; }
}

public enum CellVertexType {
    Inside,
    Outside,
    Boundary
}
