using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitKrChecker.Models.Rule;

namespace RevitKrChecker.Models.Check {
    public class CompareParamsCheck : ICheck {
        public CompareParamsCheck(string checkName,
                          string targetParamName,
                          ICheckRule checkRule,
                          string sourceParamName) {
            CheckName = checkName;
            TargetParamName = targetParamName;
            CheckRule = checkRule;
            SourceParamName = sourceParamName;
        }

        public string CheckName { get; }
        public string TargetParamName { get; }
        public ICheckRule CheckRule { get; }
        private string SourceParamName { get; }

        public bool Check(Element element, out CheckInfo info) {
            string targetParamValue = element.GetParamValue<string>(TargetParamName);
            string sourceParamValue = element.GetParamValue<string>(SourceParamName);

            if(CheckRule.Check(targetParamValue, sourceParamValue)) {
                info = null;
                return true;
            }

            info = new CheckInfo(CheckName, TargetParamName, element, GetTooltip());
            return false;
        }

        public string GetTooltip() {
            return $"{CheckName}: значение параметра \"{TargetParamName}\" {CheckRule.UnfulfilledRule} \"{SourceParamName}\"";
        }
    }
}
