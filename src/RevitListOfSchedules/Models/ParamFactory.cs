using System.Collections.Generic;

using dosymep.Bim4Everyone.SharedParams;

namespace RevitListOfSchedules.Models {
    internal class ParamFactory {

        public const string FamilyParamNumber = "Номер листа";
        public const string FamilyParamName = "Наименование спецификации";
        public const string FamilyParamRevision = "Примечание";
        public const string ScheduleName = "Ведомость спецификаций и ведомостей";

        private readonly SharedParamsConfig _sharedParamsConfig = SharedParamsConfig.Instance;
        private readonly IList<SharedParam> _sharedParamsRevision;
        private readonly IList<SharedParam> _sharedParamsRevisionValue;
        private readonly SharedParam _sharedParamNumber;

        public ParamFactory() {
            _sharedParamsRevision = GetRevisionParams();
            _sharedParamsRevisionValue = GetRevisionValueParams();
            _sharedParamNumber = _sharedParamsConfig.StampSheetNumber;
        }

        public IList<SharedParam> SharedParamsRevision => _sharedParamsRevision;
        public IList<SharedParam> SharedParamsRevisionValue => _sharedParamsRevisionValue;
        public SharedParam SharedParamNumber => _sharedParamNumber;


        private IList<SharedParam> GetRevisionParams() {

            return new List<SharedParam>() {
                _sharedParamsConfig.StampSheetRevision1,
                _sharedParamsConfig.StampSheetRevision2,
                _sharedParamsConfig.StampSheetRevision3,
                _sharedParamsConfig.StampSheetRevision4,
                _sharedParamsConfig.StampSheetRevision5,
                _sharedParamsConfig.StampSheetRevision6,
                _sharedParamsConfig.StampSheetRevision7,
                _sharedParamsConfig.StampSheetRevision8
            };
        }

        private IList<SharedParam> GetRevisionValueParams() {

            return new List<SharedParam>() {
                _sharedParamsConfig.StampSheetRevisionValue1,
                _sharedParamsConfig.StampSheetRevisionValue2,
                _sharedParamsConfig.StampSheetRevisionValue3,
                _sharedParamsConfig.StampSheetRevisionValue4,
                _sharedParamsConfig.StampSheetRevisionValue5,
                _sharedParamsConfig.StampSheetRevisionValue6,
                _sharedParamsConfig.StampSheetRevisionValue7,
                _sharedParamsConfig.StampSheetRevisionValue8
            };
        }

    }
}
