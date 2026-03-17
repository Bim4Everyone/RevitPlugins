using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;

namespace RevitVolumeModifier.Models;
internal class GenericModelSelectionFilter : ISelectionFilter {

    public bool AllowElement(Element element) {
        var category = element.Category?.GetBuiltInCategory();
        return element.Category != null && category.Value == BuiltInCategory.OST_GenericModel;
    }

    public bool AllowReference(Reference reference, XYZ position) {
        return true;
    }
}
