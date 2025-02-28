using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;

namespace RevitListOfSchedules.Models {
    internal class SheetElement {

        private readonly ViewSheet _viewSheet;
        private readonly SharedParam _sheetNumberParam;
        private readonly string _name;
        private readonly string _number;
        private readonly string _revisionNumber;

        public SheetElement(ViewSheet viewSheet) {
            _viewSheet = viewSheet;
            _sheetNumberParam = SharedParamsConfig.Instance.StampSheetNumber;
            _name = _viewSheet.Name;
            _number = SetNumberParam(_sheetNumberParam);
            _revisionNumber = GetRevisionString();
        }

        public ViewSheet Sheet => _viewSheet;
        public string Name => _name;
        public string Number => _number;
        public string RevisionNumber => _revisionNumber;

        private string SetNumberParam(SharedParam sharedParam) {
            string value = null;
            if(Sheet.IsExistsParam(sharedParam)) {
                if(Sheet.IsExistsParamValue(sharedParam)) {
                    value = Sheet.GetParamValueOrDefault<string>(sharedParam);
                }
            }
            return value;
        }

        private string GetRevisionString() {
            return "Изм.1";
        }
    }
}
