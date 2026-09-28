using Autodesk.Revit.DB;

namespace RevitEnumerateBySpline.Models;

public class CurveModel {
    public ElementId? ElementId { get; set; }
    public CurveElement? CurveElement { get; set; }
}
