// Ignore Spelling: Tooltip

using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitKrChecker.Models.Rule;
using RevitKrChecker.Models.Services;

namespace RevitKrChecker.Models.Check {
    public class ParamCheck : ICheck {
        private readonly ParamService _paramService;

        public ParamCheck(string checkName,
                          string targetParamName,
                          ParamLevel targetParamLevel,
                          ICheckRule checkRule,
                          List<string> trueValues) {
            CheckName = checkName ?? throw new ArgumentNullException(nameof(checkName));
            TargetParamName = targetParamName ?? throw new ArgumentNullException(nameof(targetParamName));
            TargetParamLevel = targetParamLevel;
            CheckRule = checkRule ?? throw new ArgumentNullException(nameof(checkRule));
            TrueValues = trueValues ?? throw new ArgumentNullException(nameof(trueValues));

            _paramService = new ParamService();
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

            var targetParams = _paramService.GetParamsToCheck(element, TargetParamName, TargetParamLevel);

            foreach(var targetParam in targetParams) {
                string targetParamValue = targetParam.AsValueString();

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
            return $"{CheckName}: значение параметра \"{TargetParamName}\" {CheckRule.UnfulfilledRule} {GetTrueValuesAsStr()}";
        }
    }
}
