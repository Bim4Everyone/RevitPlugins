using System.Text;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitListOfSchedules.Models {
    internal class SheetElement {
        private const string _revisionStartString = "Изм.1";
        private readonly ViewSheet _viewSheet;
        private readonly ParamFactory _paramFactory;
        private readonly string _name;
        private readonly string _number;
        private readonly string _revisionNumber;

        public SheetElement(ViewSheet viewSheet, ParamFactory paramFactory) {
            _viewSheet = viewSheet;
            _paramFactory = paramFactory;
            _name = _viewSheet.Name;
            _number = SetNumberParam();
            _revisionNumber = GetRevisionString();
        }

        public ViewSheet Sheet => _viewSheet;
        public string Name => _name;
        public string Number => _number;
        public string RevisionNumber => _revisionNumber;

        private string SetNumberParam() {
            return Sheet.GetParamValueOrDefault<string>(_paramFactory.SharedParamNumber, null);
        }

        private string GetRevisionString() {
            var sb = new StringBuilder();
            for(int i = 0; i < _paramFactory.SharedParamsRevision.Count; i++) {
                if(_viewSheet.IsExistsParamValue(_paramFactory.SharedParamsRevision[i])) {
                    string paramValue = _viewSheet.GetParamValue<string>(_paramFactory.SharedParamsRevision[i]);
                    string paramValueRevision = _viewSheet.GetParamValue<string>(_paramFactory.SharedParamsRevisionValue[i]);

                    sb.Append(_revisionStartString)
                      .Append(paramValue)
                      .Append($"({paramValueRevision}); ");
                }
            }
            return sb.ToString();
        }
    }
}
