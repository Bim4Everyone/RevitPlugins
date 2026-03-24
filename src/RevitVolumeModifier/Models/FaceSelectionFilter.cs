using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

internal class FaceSelectionFilter : ISelectionFilter {
    public bool AllowElement(Element elem) {
        return true;
    }

    public bool AllowReference(Reference reference, XYZ position) {
        return reference != null && reference.ElementReferenceType == ElementReferenceType.REFERENCE_TYPE_SURFACE;
    }
}
