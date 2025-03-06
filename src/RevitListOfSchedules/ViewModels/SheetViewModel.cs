using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitListOfSchedules.Models;

namespace RevitListOfSchedules.ViewModels {
    internal class SheetViewModel {

        private readonly SheetElement _sheetElement;
        private readonly LinkViewModel _linkViewModel;
        private readonly string _name;
        private readonly string _number;
        private readonly string _revisionNumber;
        private readonly ViewSheet _viewSheet;

        public SheetViewModel(SheetElement sheetElement, LinkViewModel linkViewModel = null) {
            _sheetElement = sheetElement;
            _linkViewModel = linkViewModel;
            _name = _sheetElement.Name;
            _number = _sheetElement.Number;
            _revisionNumber = _sheetElement.RevisionNumber;
            _viewSheet = _sheetElement.Sheet;
        }

        public LinkViewModel LinkViewModel => _linkViewModel;
        public string Name => _name;
        public string Number => _number;
        public string RevisionNumber => _revisionNumber;
        public ViewSheet ViewSheet => _viewSheet;
        public RevitParam SheetParameter { get; set; }
        public string AlbumName => GetAlbumName();

        private string GetAlbumName() {
            if(SheetParameter != null) {
                return _sheetElement.Sheet.GetParamValue<string>(SheetParameter);
            }
            return "Нет параметра группировки";

        }

    }
}
