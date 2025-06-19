using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace RevitArchitecturalDocumentation.Models;
internal class ScheduleSelectionFilter : ISelectionFilter {
    public bool AllowElement(Element element) {
        return element is ScheduleSheetInstance;
    }

    public bool AllowReference(Reference refer, XYZ point) {
        return false;
    }
}
