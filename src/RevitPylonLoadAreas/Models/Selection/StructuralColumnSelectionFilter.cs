using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace RevitPylonLoadAreas.Models.Selection;

internal sealed class StructuralColumnSelectionFilter : ISelectionFilter {
    private readonly ElementId _structuralColumnsId = new(BuiltInCategory.OST_StructuralColumns);

    public bool AllowElement(Element elem) {
        return elem is FamilyInstance fi
               && fi.Category != null
               && fi.Category.Id == _structuralColumnsId;
    }

    public bool AllowReference(Reference reference, XYZ position) => true;
}
