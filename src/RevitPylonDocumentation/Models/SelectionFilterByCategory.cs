using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace RevitPylonDocumentation.Models;
internal class SelectionFilterByCategory : ISelectionFilter {
    private readonly IList<ElementId> _categoryIds;

    public SelectionFilterByCategory(IList<ElementId> categoryIds) {
        _categoryIds = categoryIds;
    }

    public bool AllowElement(Element elem) {
        // Проверяем, принадлежит ли элемент к разрешенным категориям
        if(_categoryIds.Contains(elem.Category.Id)) {
            return true;
        }
        return false;
    }

    public bool AllowReference(Reference reference, XYZ position) {
        return false;
    }
}
