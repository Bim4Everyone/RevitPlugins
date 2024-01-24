using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI.Selection;

namespace RevitFinishingWalls.Services.Selection {
    /// <summary>
    /// Фильтр выбора помещений из активного файла, которые окружены и размещены
    /// </summary>
    internal class SelectionFilterRooms : ISelectionFilter {
        private readonly Document _activeDocument;


        internal SelectionFilterRooms(Document activeDocument) {
            _activeDocument = activeDocument;
        }


        public bool AllowElement(Element elem) {
            return (elem != null) && (elem is Room);
        }

        public bool AllowReference(Reference reference, XYZ position) {
            return (_activeDocument.GetElement(reference) is Room room)
                && room.Area > 0;
        }
    }
}
