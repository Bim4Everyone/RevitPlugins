using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace RevitEnumerateBySpline.Models;

internal class CurveSelectionFilter : ISelectionFilter {
    private readonly Document _doc;

    public CurveSelectionFilter(Document doc) {
        _doc = doc;
    }

    public bool AllowElement(Element elem) {
        return elem is ModelCurve or DetailCurve;
    }

    public bool AllowReference(Reference reference, XYZ position) {
        return true;
    }
}
