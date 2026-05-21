using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace RevitPackageDocumentation.Models;
internal class FloorSelectionFilter : ISelectionFilter {
    public bool AllowElement(Element element) {
        return element is Floor;
    }

    public bool AllowReference(Reference refer, XYZ point) {
        return false;
    }
}
