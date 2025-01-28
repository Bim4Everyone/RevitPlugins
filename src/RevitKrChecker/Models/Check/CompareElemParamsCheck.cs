using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitKrChecker.Models.CheckOptions;
using RevitKrChecker.Models.Interfaces;
using RevitKrChecker.Models.Services;

namespace RevitKrChecker.Models.Check {
    public class CompareElemParamsCheck : ICheck {
        private readonly ParamValueService _paramService;

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
            SourceParamLevel = checkOptions.SourceParamLevel;

            _paramService = new ParamValueService();
        }

        public string CheckName { get; }
        public string TargetParamName { get; }
        public ParamLevel TargetParamLevel { get; }
        public ICheckRule CheckRule { get; }
        private string SourceParamName { get; }
        public ParamLevel SourceParamLevel { get; }

        private bool CheckAllValues(string targetParamValue, List<string> sourceParamValues) {
            return sourceParamValues.All(sourceParamValue => CheckRule.Check(targetParamValue, sourceParamValue));
        }

        public bool Check(Element element, out CheckInfo info) {
            if(element == null)
                throw new ArgumentNullException(nameof(element));

            string targetParamValue = _paramService.GetParamValueToCheck(element, TargetParamName, TargetParamLevel);
            List<string> sourceParamValues = _paramService.GetParamValuesToCheck(element, SourceParamName, SourceParamLevel);

            // Значение проверяемого параметра должно соответствовать каждому из значений (может быть несколько,
            // если материалов несколько)
            if(!CheckAllValues(targetParamValue, sourceParamValues)) {
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
