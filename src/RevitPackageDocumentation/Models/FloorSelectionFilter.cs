using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace RevitPackageDocumentation.Models;

/// <summary>
/// Фильтр выбора в модели: для параметра "Выбор элемента" разрешены только перекрытия
/// </summary>
internal class FloorSelectionFilter : ISelectionFilter {
    public bool AllowElement(Element element) {
        return element is Floor;
    }

    public bool AllowReference(Reference refer, XYZ point) {
        return false;
    }
}
