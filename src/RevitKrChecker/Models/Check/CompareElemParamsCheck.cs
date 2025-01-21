using System;

using Autodesk.Revit.DB;

using RevitKrChecker.Models.CheckOptions;
using RevitKrChecker.Models.Interfaces;
using RevitKrChecker.Models.Services;

namespace RevitKrChecker.Models.Check {
    public class CompareElemParamsCheck : ICheck {
        private readonly ParamService _paramService;

        public CompareElemParamsCheck(CompareCheckOptions checkOptions) {
            CheckName = checkOptions.CheckName
                ?? throw new ArgumentNullException(nameof(checkOptions.CheckName));
            TargetParamName = checkOptions.TargetParamName
                ?? throw new ArgumentNullException(nameof(checkOptions.TargetParamName));
            TargetParamLevel = checkOptions.TargetParamLevel is ParamLevel.Material
                ? throw new ArgumentException("Проверка не предусмотрена для проверки параметра материала")
                : checkOptions.TargetParamLevel;

            CheckRule = checkOptions.CheckRule
                ?? throw new ArgumentNullException(nameof(checkOptions.CheckRule));
            SourceParamName = checkOptions.SourceParamName
                ?? throw new ArgumentNullException(nameof(checkOptions.SourceParamName));
            SourceParamLevel = checkOptions.SourceParamLevel is ParamLevel.Material
                ? throw new ArgumentException("Проверка не предусмотрена для сравнения с параметром материала")
                : checkOptions.SourceParamLevel;

            _paramService = new ParamService();
        }

        public string CheckName { get; }
        public string TargetParamName { get; }
        public ParamLevel TargetParamLevel { get; }
        public ICheckRule CheckRule { get; }
        private string SourceParamName { get; }
        public ParamLevel SourceParamLevel { get; }


        public bool Check(Element element, out CheckInfo info) {
            Parameter targetParam = _paramService.GetParamToCheck(element, TargetParamName, TargetParamLevel);
            Parameter sourceParam = _paramService.GetParamToCheck(element, SourceParamName, SourceParamLevel);

            string targetParamValue = targetParam.AsValueString();
            string sourceParamValue = sourceParam.AsValueString();

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
