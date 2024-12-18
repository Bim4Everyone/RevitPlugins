using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitKrChecker.Models.Rule;

namespace RevitKrChecker.Models.Check {
    public class TemplatesCompareParamsCheck : ICheck {

        public TemplatesCompareParamsCheck(string checkName,
                          string targetParamName,
                          ICheckRule checkRule,
                          string sourceParamName,
                          Dictionary<string, string> dictForCompare) {
            CheckName = checkName;
            TargetParamName = targetParamName;
            CheckRule = checkRule;
            SourceParamName = sourceParamName;
            DictForCompare = dictForCompare;
        }

        public string CheckName { get; }
        public string TargetParamName { get; }
        public ICheckRule CheckRule { get; }
        private string SourceParamName { get; }
        private Dictionary<string, string> DictForCompare { get; }


        public bool Check(Element element, out CheckInfo info) {
            string targetParamValue = element.GetParamValue<string>(TargetParamName);
            string sourceParamValue = DictForCompare[element.GetParamValue<string>(SourceParamName)];

            if(CheckRule.Check(targetParamValue, sourceParamValue)) {
                info = null;
                return true;
            }

            info = new CheckInfo(CheckName, TargetParamName, element, GetTooltip());
            return false;
        }

        public string GetTooltip() {
            return $"{CheckName}: значение параметра \"{TargetParamName}\" {CheckRule.UnfulfilledRule}" +
                $" \"{SourceParamName}\" по правилу: {GetDictForCompareAsStr()}";
        }

        private string GetDictForCompareAsStr() {
            string answer = string.Empty;
            string separator = ", ";

            foreach(string dictKey in DictForCompare.Keys) {
                answer += $"{dictKey}: {DictForCompare[dictKey]}{separator}";
            }
            answer = answer.Substring(0, answer.LastIndexOf(separator));
            return answer;
        }
    }
}
