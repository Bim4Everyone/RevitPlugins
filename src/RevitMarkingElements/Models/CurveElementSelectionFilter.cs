using Autodesk.Revit.DB;

namespace RevitMarkingElements.Models;
public class CurveElementSelectionFilter : Autodesk.Revit.UI.Selection.ISelectionFilter {
    public bool AllowElement(Element element) {
        return element is ModelLine or ModelNurbSpline;
    }

    public bool AllowReference(Reference reference, XYZ position) {
        return false;
    }
}
