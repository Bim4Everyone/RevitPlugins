using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;

namespace RevitPunchingRebar.Models.SelectionFilters;

/// <summary>
/// Фильтр выбора пилонов и прямоугольных несущих колонн в текущем проекте.
/// </summary>
internal class PylonFromModelFilter : ISelectionFilter {
    public bool AllowElement(Element elem) {
        return IsSupportedColumn(elem);
    }

    public bool AllowReference(Reference reference, XYZ position) {
        return false;
    }

    public static bool IsSupportedColumn(Element element) {
        if(element is not FamilyInstance familyInstance
           || !familyInstance.InAnyCategory(BuiltInCategory.OST_StructuralColumns)
           || familyInstance.IsSlantedColumn
           || familyInstance.Location is not LocationPoint) {
            return false;
        }

        var (length, width) = StructColumnPylon.GetSectionDimensions(familyInstance.Symbol);
        return length > 0 && width > 0;
    }
}
