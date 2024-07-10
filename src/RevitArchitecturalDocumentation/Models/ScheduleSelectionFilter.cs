using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace RevitArchitecturalDocumentation.Models {
    internal class ScheduleSelectionFilter : ISelectionFilter {
        public bool AllowElement(Element element) {
            if(element is ScheduleSheetInstance) {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference refer, XYZ point) {
            return false;
        }
    }
}
