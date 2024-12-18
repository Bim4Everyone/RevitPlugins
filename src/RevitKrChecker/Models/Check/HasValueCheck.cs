using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitKrChecker.Models.Check {
    public class HasValueCheck : ICheck {
        public HasValueCheck(string checkName,
                          string targetParamName) {
            CheckName = checkName;
            TargetParamName = targetParamName;
        }

        public string CheckName { get; }
        public string TargetParamName { get; }

        public bool Check(Element element, out CheckInfo info) {
            string targetParamValue = element.GetParamValue<string>(TargetParamName);

            if(string.IsNullOrEmpty(targetParamValue)) {
                info = null;
                return true;
            }

            info = new CheckInfo(CheckName, TargetParamName, element, GetTooltip());
            return false;
        }

        public string GetTooltip() {
            return $"{CheckName}: значение параметра \"{TargetParamName}\" отсутствует";
        }
    }
}
