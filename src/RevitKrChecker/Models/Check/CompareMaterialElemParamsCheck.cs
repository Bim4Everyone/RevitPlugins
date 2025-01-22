using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitKrChecker.Models.CheckOptions;
using RevitKrChecker.Models.Interfaces;
using RevitKrChecker.Models.Services;

namespace RevitKrChecker.Models.Check {
    public class CompareMaterialElemParamsCheck : ICheck {
        private readonly ParamValueService _paramService;

        public CompareMaterialElemParamsCheck(CompareCheckOptions checkOptions) {
            CheckName = checkOptions.CheckName
                ?? throw new ArgumentNullException(nameof(checkOptions.CheckName));
            TargetParamName = checkOptions.TargetParamName
                ?? throw new ArgumentNullException(nameof(checkOptions.TargetParamName));
            // Проверяем, что параметр для проверке на уровне материала
            if(checkOptions.TargetParamLevel != ParamLevel.Material)
                throw new ArgumentException("Проверка предусмотрена только для проверки параметра материала");

            CheckRule = checkOptions.CheckRule
                ?? throw new ArgumentNullException(nameof(checkOptions.CheckRule));
            SourceParamName = checkOptions.SourceParamName
                ?? throw new ArgumentNullException(nameof(checkOptions.SourceParamName));
            SourceParamLevel = checkOptions.SourceParamLevel is ParamLevel.Material
                ? throw new ArgumentException("Проверка не предусмотрена для проверки c параметрами материала")
                : checkOptions.SourceParamLevel;

            _paramService = new ParamValueService();
        }

        public string CheckName { get; }
        public string TargetParamName { get; }
        public ICheckRule CheckRule { get; }
        private string SourceParamName { get; }
        public ParamLevel SourceParamLevel { get; }

        public bool Check(Element element, out CheckInfo info) {
            if(element == null)
                throw new ArgumentNullException(nameof(element));
            Document doc = element.Document;
            List<Element> materials = element.GetMaterialIds(false)
                           .Select(id => doc.GetElement(id))
                           .ToList();
            string sourceParamValue = _paramService.GetParamValueToCheck(element, SourceParamName, SourceParamLevel);

            foreach(Element material in materials) {
                string targetParamValue = material.GetParam(TargetParamName).AsValueString();
                if(!CheckRule.Check(targetParamValue, sourceParamValue)) {
                    info = new CheckInfo(CheckName, TargetParamName, element, GetTooltip());
                    return false;
                }
            }
            info = null;
            return true;
        }

        public string GetTooltip() {
            return $"{CheckName}: значение параметра \"{TargetParamName}\" {CheckRule.UnfulfilledRule} \"{SourceParamName}\"";
        }
    }
}
