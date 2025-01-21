// Ignore Spelling: Tooltip

using System;

using Autodesk.Revit.DB;

using RevitKrChecker.Models.Rule;
using RevitKrChecker.Models.Services;

namespace RevitKrChecker.Models.Check {
    public class CompareElemParamsCheck : ICheck {
        private readonly ParamService _paramService;

        public CompareElemParamsCheck(string checkName,
                          string targetParamName,
                          ParamLevel targetParamLevel,
                          ICheckRule checkRule,
                          string sourceParamName,
                          ParamLevel sourceParamLevel) {
            CheckName = checkName ?? throw new ArgumentNullException(nameof(checkName));
            TargetParamName = targetParamName ?? throw new ArgumentNullException(nameof(targetParamName));
            TargetParamLevel = targetParamLevel;
            CheckRule = checkRule ?? throw new ArgumentNullException(nameof(checkRule));
            SourceParamName = sourceParamName ?? throw new ArgumentNullException(nameof(sourceParamName));
            SourceParamLevel = sourceParamLevel;

            _paramService = new ParamService();
        }

        public string CheckName { get; }
        public string TargetParamName { get; }
        public ParamLevel TargetParamLevel { get; }
        public ICheckRule CheckRule { get; }
        private string SourceParamName { get; }
        public ParamLevel SourceParamLevel { get; }


        public bool Check(Element element, out CheckInfo info) {
            Document doc = element.Document;

            Parameter targetParam = _paramService.GetParamToCheck(element, TargetParamName, TargetParamLevel);
            Parameter sourceParam = _paramService.GetParamToCheck(element, SourceParamName, SourceParamLevel);

            string targetParamValue = targetParam.AsValueString();
            string sourceParamValue = targetParam.AsValueString();

            if(!CheckRule.Check(targetParamValue, sourceParamValue)) {
                info = new CheckInfo(CheckName, TargetParamName, element, GetTooltip());
                return false;
            }

            info = null;
            return true;
        }


        public string GetTooltip() {
            return $"{CheckName}: значение параметра \"{TargetParamName}\" {CheckRule.UnfulfilledRule} \"{SourceParamName}\"";
        }
    }
}
