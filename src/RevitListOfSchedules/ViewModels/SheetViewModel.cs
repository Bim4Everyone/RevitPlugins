using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitListOfSchedules.Models;

namespace RevitListOfSchedules.ViewModels {
    internal class SheetViewModel {
        private readonly ILocalizationService _localizationService;
        private readonly SheetElement _sheetElement;
        private readonly LinkViewModel _linkViewModel;
        private readonly ViewSheet _viewSheet;
        private readonly string _name;
        private readonly string _number;
        private readonly string _revisionNumber;

        public SheetViewModel(
            ILocalizationService localizationService,
            SheetElement sheetElement,
            LinkViewModel linkViewModel = null) {

            _localizationService = localizationService;
            _sheetElement = sheetElement;
            _linkViewModel = linkViewModel;
            _viewSheet = _sheetElement.Sheet;
            _name = _sheetElement.Name;
            _number = _sheetElement.Number;
            _revisionNumber = _sheetElement.RevisionNumber;
        }

        public LinkViewModel LinkViewModel => _linkViewModel;
        public ViewSheet ViewSheet => _viewSheet;
        public RevitParam GroupParameter { get; set; }
        public string Name => _name;
        public string Number => _number;
        public string RevisionNumber => _revisionNumber;
        public string AlbumName => GetAlbumName();

        private string GetAlbumName() {
            if(GroupParameter != null) {
                return _sheetElement.Sheet.GetParamValue<string>(GroupParameter);
            }
            return _localizationService.GetLocalizedString("GroupParameter.NoParameter");
        }
    }
}
