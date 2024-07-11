using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;

namespace RevitOpeningSlopes.Models {
    internal class WindowSelectionFilter : ISelectionFilter {

        public bool AllowElement(Element elem) {
            return elem != null
                && elem is FamilyInstance
                && ElementExtensions.InAnyCategory(elem, BuiltInCategory.OST_Windows);
        }

        public bool AllowReference(Reference reference, XYZ position) {
            return false;
        }
    }
}
