using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace RevitPylonLoadAreas.Models.Selection;

internal sealed class WallSelectionFilter : ISelectionFilter {
    public bool AllowElement(Element elem) {
        return elem is Wall;
    }

    public bool AllowReference(Reference reference, XYZ position) {
        return true;
    }
}
