using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI.Selection;

namespace RevitRemoveRoomTags.Models;
internal class RoomTagSelectionFilter : ISelectionFilter {
    public bool AllowElement(Element element) {
        return element is RoomTag;
    }

    public bool AllowReference(Reference refer, XYZ point) {
        return false;
    }
}
