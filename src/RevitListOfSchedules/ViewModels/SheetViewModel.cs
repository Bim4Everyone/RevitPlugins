using Autodesk.Revit.DB;

using RevitListOfSchedules.Models;

namespace RevitListOfSchedules.ViewModels {
    internal class SheetViewModel {

        private readonly SheetElement _sheetElement;

        private readonly ElementId _linkTypeId;
        private readonly string _name;
        private readonly string _number;
        private readonly string _revisionNumber;


        public SheetViewModel(SheetElement sheetElement, ElementId linkTypeId = null) {
            _sheetElement = sheetElement;
            _linkTypeId = linkTypeId;
            _name = _sheetElement.Name;
            _number = _sheetElement.Number;
            _revisionNumber = _sheetElement.RevisionNumber;
        }

        public ElementId LinkTypeId => _linkTypeId;
        public string Name => _name;
        public string Number => _number;
        public string RevisionNumber => _revisionNumber;
        public Parameter SheetParameter { get; set; }
        public string AlbumName => GetAlbumName();


        private string GetAlbumName() {
            return _sheetElement.Sheet.LookupParameter(SheetParameter.Definition.Name).AsString();
        }
    }
}
