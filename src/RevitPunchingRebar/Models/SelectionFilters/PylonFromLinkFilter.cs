using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace RevitPunchingRebar.Models.SelectionFilters;

/// <summary>
/// Фильтр выбора пилонов в связанных файлах.
/// </summary>
internal class PylonFromLinkFilter : ISelectionFilter {
    private readonly Document _hostDoc;

    public PylonFromLinkFilter(Document hostDoc) {
        _hostDoc = hostDoc;
    }

    public bool AllowElement(Element elem) {
        return elem is RevitLinkInstance;
    }

    public bool AllowReference(Reference reference, XYZ position) {
        // У выгруженной связи документа нет - элементы из нее выбрать нельзя
        Document linkedDoc = (_hostDoc.GetElement(reference.ElementId) as RevitLinkInstance)?.GetLinkDocument();
        Element linkedElem = linkedDoc?.GetElement(reference.LinkedElementId);

        return linkedElem != null && PylonFromModelFilter.IsPylon(linkedElem);
    }
}
