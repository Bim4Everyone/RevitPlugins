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
            string value = null;
            if(Sheet.IsExistsParam(_paramFactory.SharedParamNumber)) {
                if(Sheet.IsExistsParamValue(_paramFactory.SharedParamNumber)) {
                    value = Sheet.GetParamValueOrDefault<string>(_paramFactory.SharedParamNumber);
                }
            }
            return value;
        }

        private string GetRevisionString() {
            string resultstringrevision = string.Empty;
            for(int i = 0; i < _paramFactory.SharedParamsRevision.Count; i++) {
                if(_viewSheet.IsExistsParam(_paramFactory.SharedParamsRevision[i])) {
                    if(_viewSheet.IsExistsParamValue(_paramFactory.SharedParamsRevision[i])) {
                        string paramValue = _viewSheet.GetParamValue<string>(_paramFactory.SharedParamsRevision[i]);
                        string paramValueRevision = _viewSheet.GetParamValue<string>(_paramFactory.SharedParamsRevisionValue[i]);
                        resultstringrevision += string.Concat(_revisionStartString, paramValue, $"({paramValueRevision}); ");
                    }
                }
            }
            return resultstringrevision;
        }
    }
}
