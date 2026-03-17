using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace RevitVolumeModifier.Models;
internal class FaceSelectionFilter : ISelectionFilter {
    public bool AllowElement(Element elem) {
        return true; // Элемент разрешаем, проверка на грань делается ниже
    }

    public bool AllowReference(Reference reference, XYZ position) {
        return reference?.ElementReferenceType == ElementReferenceType.REFERENCE_TYPE_SURFACE;
    }
}
