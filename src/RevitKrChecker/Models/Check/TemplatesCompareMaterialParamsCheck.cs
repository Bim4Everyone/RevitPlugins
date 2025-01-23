using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitKrChecker.Models.CheckOptions;
using RevitKrChecker.Models.Interfaces;
using RevitKrChecker.Models.Services;

namespace RevitKrChecker.Models.Check {
    public class TemplatesCompareMaterialParamsCheck : ICheck {
        private readonly ParamValueService _paramService;

        public TemplatesCompareMaterialParamsCheck(TemplatesCompareCheckOptions checkOptions) {
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
            SourceParamLevel = checkOptions.SourceParamLevel != ParamLevel.Material
                ? throw new ArgumentException("Проверка не предусмотрена для проверки c параметрами не материала")
                : checkOptions.SourceParamLevel;

            DictForCompare = checkOptions.DictForCompare
                ?? throw new ArgumentNullException(nameof(checkOptions.DictForCompare));
            DictForCompareRule = checkOptions.DictForCompareRule;

            _paramService = new ParamValueService();
        }

        public string CheckName { get; }
        public string TargetParamName { get; }
        public ICheckRule CheckRule { get; }
        private string SourceParamName { get; }
        public ParamLevel SourceParamLevel { get; }
        public Dictionary<string, string> DictForCompare { get; }
        public ICheckRule DictForCompareRule { get; }


        private string GetDictForCompareKeyByRule(string value) {
            return DictForCompare.Keys.FirstOrDefault(key => DictForCompareRule.Check(value, key));
        }

        private string GetValueByDict(string value) {
            string key = GetDictForCompareKeyByRule(value);
            return key is null ? null : DictForCompare[key];
        }

        public bool Check(Element element, out CheckInfo info) {
            if(element == null)
                throw new ArgumentNullException(nameof(element));
            Document doc = element.Document;
            List<Element> materials = element.GetMaterialIds(false)
                           .Select(id => doc.GetElement(id))
                           .ToList();

            foreach(Element material in materials) {
                string targetParamValue = material.GetParam(TargetParamName).AsValueString();
                string sourceParamValue = material.GetParam(SourceParamName).AsValueString();
                string sourceParamValueByDict = GetValueByDict(sourceParamValue);

                if(!CheckRule.Check(targetParamValue, sourceParamValueByDict)) {
                    info = new CheckInfo(CheckName, TargetParamName, element, GetTooltip());
                    return false;
                }
            }
            info = null;
            return true;
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

        public string GetTooltip() {
            return $"{CheckName}: значение параметра \"{TargetParamName}\" {CheckRule.UnfulfilledRule}" +
                $" \"{SourceParamName}\" по правилу: {GetDictForCompareAsStr()}";
        }
    }
}
