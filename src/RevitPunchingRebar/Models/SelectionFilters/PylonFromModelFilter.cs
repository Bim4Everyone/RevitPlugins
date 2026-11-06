using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;

namespace RevitPunchingRebar.Models.SelectionFilters;

/// <summary>
/// Фильтр выбора пилонов в текущем проекте.
/// </summary>
internal class PylonFromModelFilter : ISelectionFilter {
    // Часть имени семейства, по которой несущая колонна считается пилоном
    private const string _pylonFamilyNamePart = "пилон";

    public bool AllowElement(Element elem) {
        return IsPylon(elem);
    }

    public bool AllowReference(Reference reference, XYZ position) {
        return false;
    }

    /// <summary>
    /// Проверяет, что элемент — несущая колонна, в имени семейства которой есть «пилон».
    /// </summary>
    /// <remarks>
    /// Категория проверяется по BuiltInCategory, поэтому проверка работает в любой языковой версии Revit.
    /// </remarks>
    public static bool IsPylon(Element element) {
        return element is FamilyInstance familyInstance
               && familyInstance.Category?.GetBuiltInCategory() == BuiltInCategory.OST_StructuralColumns
               && familyInstance.Symbol.FamilyName.IndexOf(
                   _pylonFamilyNamePart, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
