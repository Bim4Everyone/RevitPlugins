using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SystemParams;

namespace RevitClashDetective.Models.Utils {
    internal class DoubleValueParser {
        private readonly RevitRepository _revitRepository;
        private readonly RevitParam _revitParam;

        public DoubleValueParser(RevitRepository revitRepository, RevitParam revitParam) {
            _revitRepository = revitRepository;
            _revitParam = revitParam;
        }

        public bool TryParse(string value, out double result) {
#if D2020 || R2020
            var type = GetUnitType();
            if(type == UnitType.UT_Undefined) {
                return double.TryParse(value, out result);
            }
            return UnitFormatUtils.TryParse(new Units(UnitSystem.Metric), type, value, out result);
#else
            var type = GetUnitType();
            if(string.IsNullOrEmpty(type.TypeId)) {
                return double.TryParse(value, out result);
            }
            return UnitFormatUtils.TryParse(new Units(UnitSystem.Metric), type, value, out result);
#endif
        }


#if D2020 || R2020
        private UnitType GetUnitType() {
            if(_revitParam is SystemParam p) {
                return UnitTypeUtils.GetUnitType(p.SystemParamId);
            } else {
                var paramElement = _revitRepository.GetDocuments().Select(item => _revitParam.GetRevitParamElement(item)).FirstOrDefault(item => item != null);
                if(paramElement != null) {
                    return paramElement.GetDefinition().UnitType;
                }
            }
            return UnitType.UT_Undefined;
        }
#else
        private ForgeTypeId GetUnitType() {
            if(_revitParam is SystemParam p) {
                return UnitTypeUtils.GetUnitType(p.SystemParamId);
            } else {
                var paramElement = _revitRepository.GetDocuments().Select(item => _revitParam.GetRevitParamElement(item)).FirstOrDefault(item => item != null);
                if(paramElement != null) {
#if D2021 || R2021
                    return paramElement.GetDefinition().GetSpecTypeId();
#else
                    return paramElement.GetDefinition().GetDataType();
#endif
                }
            }
            return new ForgeTypeId();
        }
#endif
    }
}
