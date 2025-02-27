using Autodesk.Revit.DB;

using RevitListOfSchedules.Models;

namespace RevitListOfSchedules.ViewModels {
    internal class SheetViewModel {

        private readonly SheetElement _sheetElement;
        private readonly ElementId _linkTypeId;

        public SheetViewModel(SheetElement sheetElement, ElementId linkTypeId = null) {
            _sheetElement = sheetElement;
            _linkTypeId = linkTypeId;
        }

        public string Name => _sheetElement.Name;
        public string Number => _sheetElement.Number;
        public string AlbumName => GetAlbumName();
        public string RevisionNumber => _sheetElement.RevisionNumber;
        public ElementId LinkTypeId => _linkTypeId;
        public Parameter SheetParameter { get; set; }

        private string GetAlbumName() {
            return _sheetElement.Sheet.LookupParameter(SheetParameter.Definition.Name).AsString();
        }
    }
}
