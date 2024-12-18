using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitKrChecker.Models.Rule;

namespace RevitKrChecker.Models.Check {
    public class ParamCheck : ICheck {

        public ParamCheck(string checkName,
                          string targetParamName,
                          ICheckRule checkRule,
                          List<string> trueValues) {
            CheckName = checkName;
            TargetParamName = targetParamName;
            CheckRule = checkRule;
            TrueValues = trueValues;
        }

        public string CheckName { get; }
        public string TargetParamName { get; }
        public ICheckRule CheckRule { get; }
        public List<string> TrueValues { get; }


        public bool Check(Element element, out CheckInfo info) {
            string targetParamValue = element.GetParamValue<string>(TargetParamName);

            if(TrueValues is null) {
                throw new ArgumentNullException("The correct values were not passed.");
            }

            foreach(var trueValue in TrueValues) {
                if(CheckRule.Check(targetParamValue, trueValue)) {
                    info = null;
                    return true;
                }
            }

            info = new CheckInfo(CheckName, TargetParamName, element, GetTooltip());
            return false;
        }

        public string GetTooltip() {
            return $"{CheckName}: значение параметра \"{TargetParamName}\" {CheckRule.UnfulfilledRule} {GetTrueValuesAsStr()}";
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
    }
}
