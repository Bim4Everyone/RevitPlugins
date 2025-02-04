using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitKrChecker.Models.CheckOptions;
using RevitKrChecker.Models.Interfaces;
using RevitKrChecker.Models.Services;

namespace RevitKrChecker.Models.Check {
    public class ParamCheck : ICheck {
        private readonly ILocalizationService _localizationService;
        private readonly ParamValueService _paramService;

        public ParamCheck(ParamCheckOptions checkOptions,
                          ILocalizationService localizationService,
                          ParamValueService paramValueService) {
            _localizationService = localizationService;
            _paramService = paramValueService;

            CheckName = checkOptions.CheckName
                ?? throw new ArgumentNullException(nameof(checkOptions.CheckName));

            TargetParamName = checkOptions.TargetParamName
                ?? throw new ArgumentNullException(nameof(checkOptions.TargetParamName));
            TargetParamLevel = checkOptions.TargetParamLevel;

            CheckRule = checkOptions.CheckRule
                ?? throw new ArgumentNullException(nameof(checkOptions.CheckRule));
            TrueValues = checkOptions.TrueValues
                ?? throw new ArgumentNullException(nameof(checkOptions.TrueValues));
        }


        public string CheckName { get; }
        public string TargetParamName { get; }
        public ParamLevel TargetParamLevel { get; }
        public ICheckRule CheckRule { get; }
        public List<string> TrueValues { get; }

        private bool CheckAnyTrueValues(string targetParamValue) {
            return TrueValues.Any(trueValue => CheckRule.Check(targetParamValue, trueValue));
        }

        public bool Check(Element element, out CheckInfo info) {
            if(element == null)
                throw new ArgumentNullException(nameof(element));

            List<string> targetParamValues = _paramService.GetParamValuesToCheck(element, TargetParamName, TargetParamLevel);

            // Каждый из значений (может быть несколько, если материалов несколько) должен соответствовать любому
            foreach(var targetParamValue in targetParamValues) {
                if(!CheckAnyTrueValues(targetParamValue)) {
                    info = new CheckInfo(CheckName, TargetParamName, element, GetTooltip());
                    return false;
                }
            }
            info = null;
            return true;
        }

        private string GetTrueValuesAsStr() {
            string answer = string.Empty;
            string separator = ", ";

            foreach(string trueValue in TrueValues) {
                answer += $"{trueValue}{separator}";
            }
            answer = answer.Substring(0, answer.LastIndexOf(separator));
            return answer;
        }

        public string GetTooltip() {
            // "значение параметра"
            var parameterValue = _localizationService.GetLocalizedString("ReportWindow.ParameterValue");
            return $"{CheckName}: {parameterValue} \"{TargetParamName}\" {CheckRule.UnfulfilledRule} {GetTrueValuesAsStr()}";
        }
    }
}
