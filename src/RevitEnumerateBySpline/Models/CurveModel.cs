using Autodesk.Revit.DB;

namespace RevitEnumerateBySpline.Models;

/// <summary>Выбранная кривая Revit для нумерации.</summary>
public class CurveModel {
    public ElementId? ElementId { get; set; }
    public CurveElement? CurveElement { get; set; }
}
