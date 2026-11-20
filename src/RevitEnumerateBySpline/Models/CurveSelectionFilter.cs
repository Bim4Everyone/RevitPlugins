using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace RevitEnumerateBySpline.Models;

/// <summary>Разрешает выбирать линии модели и линии детализации.</summary>
internal class CurveSelectionFilter : ISelectionFilter {
    public bool AllowElement(Element elem) {
        return elem is ModelCurve or DetailCurve;
    }

    public bool AllowReference(Reference reference, XYZ position) {
        return true;
    }
}
