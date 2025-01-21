using System;

using Autodesk.Revit.DB;

using RevitKrChecker.Models.CheckOptions;
using RevitKrChecker.Models.Interfaces;
using RevitKrChecker.Models.Services;

namespace RevitKrChecker.Models.Check {
    public class HasValueCheck : ICheck {
        private readonly ParamService _paramService;

        public HasValueCheck(HasValueCheckOptions checkOptions) {
            CheckName = checkOptions.CheckName ?? throw new ArgumentNullException(nameof(checkOptions.CheckName));
            TargetParamName = checkOptions.TargetParamName ?? throw new ArgumentNullException(nameof(checkOptions.TargetParamName));
            TargetParamLevel = checkOptions.TargetParamLevel;

            _paramService = new ParamService();
        }

        public string CheckName { get; }
        public string TargetParamName { get; }
        public ParamLevel TargetParamLevel { get; }


        public bool Check(Element element, out CheckInfo info) {
            if(element == null)
                throw new ArgumentNullException(nameof(element));

            var targetParams = _paramService.GetParamsToCheck(element, TargetParamName, TargetParamLevel);

            foreach(var targetParam in targetParams) {
                string targetParamValue = targetParam.AsValueString();

                if(string.IsNullOrEmpty(targetParamValue)) {
                    info = new CheckInfo(CheckName, TargetParamName, element, GetTooltip());
                    return false;
                }
            }
            info = null;
            return true;
        }

        public string GetTooltip() {
            return $"{CheckName}: значение параметра \"{TargetParamName}\" отсутствует";
        }
    }
}
