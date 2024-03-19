using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;

namespace RevitOpeningSlopes.Models {
    internal class WindowSelectionFilter : ISelectionFilter {

        public bool AllowElement(Element elem) {
            return elem.Category.GetBuiltInCategory() == BuiltInCategory.OST_Windows;
        }

        public bool AllowReference(Reference reference, XYZ position) {
            return true;
        }
    }
}
