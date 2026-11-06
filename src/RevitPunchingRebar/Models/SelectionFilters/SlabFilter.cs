using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;

namespace RevitPunchingRebar.Models.SelectionFilters;

/// <summary>
/// Фильтр выбора плиты: перекрытия или фундаментной плиты.
/// </summary>
internal class SlabFilter : ISelectionFilter {
    /// <remarks>
    /// Принимаются только элементы класса Floor: отдельные фундаменты (FamilyInstance)
    /// в категории «Фундамент несущей конструкции» плитой не являются.
    /// </remarks>
    public bool AllowElement(Element elem) {
        BuiltInCategory? category = elem.Category?.GetBuiltInCategory();

        return elem is Floor
               && (category == BuiltInCategory.OST_Floors || category == BuiltInCategory.OST_StructuralFoundation);
    }

    public bool AllowReference(Reference reference, XYZ position) {
        return false;
    }
}
