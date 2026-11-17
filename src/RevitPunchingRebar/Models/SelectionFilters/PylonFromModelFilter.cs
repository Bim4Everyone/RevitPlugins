using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
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
    /// Для выбора требуются положительные длина и ширина, заданные в типе пилона.
    /// Категория проверяется по BuiltInCategory, поэтому проверка работает в любой языковой версии Revit.
    /// </remarks>
    public static bool IsPylon(Element element) {
        if(element is not FamilyInstance familyInstance
           || !familyInstance.InAnyCategory(BuiltInCategory.OST_StructuralColumns)
           || familyInstance.Symbol.FamilyName.IndexOf(
               _pylonFamilyNamePart, StringComparison.OrdinalIgnoreCase) < 0) {
            return false;
        }

        Element elementType = familyInstance.GetElementType();
        if(elementType is null) {
            return false;
        }

        double length = elementType.GetParamValueOrDefault<double>(SharedParamsConfig.Instance.SizeLength, 0);
        double width = elementType.GetParamValueOrDefault<double>(SharedParamsConfig.Instance.SizeWidth, 0);
        return length > 0 && width > 0;
    }
}
