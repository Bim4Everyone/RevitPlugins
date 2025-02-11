using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitKrChecker.Models.CheckOptions;
using RevitKrChecker.Models.Interfaces;
using RevitKrChecker.Models.Services;

namespace RevitKrChecker.Models.Check {
    public class TemplatesCompareMaterialElemParamsCheck : ICheck {
        private readonly ILocalizationService _localizationService;
        private readonly ParamValueService _paramService;

        public TemplatesCompareMaterialElemParamsCheck(
            TemplatesCompareCheckOptions checkOptions,
            ILocalizationService localizationService,
            ParamValueService paramValueService) {

            _localizationService = localizationService;
            _paramService = paramValueService;

            CheckName = checkOptions.CheckName
                ?? throw new ArgumentNullException(nameof(checkOptions.CheckName));

            TargetParamName = checkOptions.TargetParamName
                ?? throw new ArgumentNullException(nameof(checkOptions.TargetParamName));
            // Проверяем, что параметр для проверке на уровне материала
            TargetParamLevel = checkOptions.TargetParamLevel != ParamLevel.Material
                ? throw new ArgumentException(
                    _localizationService.GetLocalizedString("ReportWindow.CheckNotForElementParameter"))
                : checkOptions.TargetParamLevel;

            CheckRule = checkOptions.CheckRule
                ?? throw new ArgumentNullException(nameof(checkOptions.CheckRule));

            SourceParamName = checkOptions.SourceParamName
                ?? throw new ArgumentNullException(nameof(checkOptions.SourceParamName));
            SourceParamLevel = checkOptions.SourceParamLevel is ParamLevel.Material
                ? throw new ArgumentException(
                    _localizationService.GetLocalizedString("ReportWindow.CheckNotForCompareWithMaterialParameter"))
                : checkOptions.SourceParamLevel;

            DictForCompare = checkOptions.DictForCompare
                ?? throw new ArgumentNullException(nameof(checkOptions.DictForCompare));
            DictForCompareRule = checkOptions.DictForCompareRule;
        }

        public string CheckName { get; }
        public string TargetParamName { get; }
        public ParamLevel TargetParamLevel { get; }
        public ICheckRule CheckRule { get; }
        private string SourceParamName { get; }
        public ParamLevel SourceParamLevel { get; }
        public Dictionary<string, string> DictForCompare { get; }
        public ICheckRule DictForCompareRule { get; }


        public bool Check(Element element, out CheckInfo info) {
            if(element == null) {
                throw new ArgumentNullException(nameof(element));
            }

            Document doc = element.Document;
            List<Element> materials = element.GetMaterialIds(false)
                           .Select(id => doc.GetElement(id))
                           .ToList();

            string sourceParamValue = _paramService.GetParamValueToCheck(element, SourceParamName, SourceParamLevel);
            string sourceParamValueByDict =
                _paramService.GetValueByDict(DictForCompare, DictForCompareRule, sourceParamValue);

            foreach(Element material in materials) {
                string targetParamValue = material.GetParam(TargetParamName).AsValueString();
                if(!CheckRule.Check(targetParamValue, sourceParamValueByDict)) {
                    info = new CheckInfo(CheckName, TargetParamName, element, GetTooltip());
                    return false;
                }
            }
            info = null;
            return true;
        }


        private string GetDictForCompareAsStr() {
            string separator = ", ";

            List<string> pairs = DictForCompare.Keys
                .Select(key => $"\"{key}\": \"{DictForCompare[key]}\"")
                .ToList();
            return string.Join(separator, pairs);
        }

        public string GetTooltip() {
            // "значение параметра"
            var parameterValue = _localizationService.GetLocalizedString("ReportWindow.ParameterValue");
            // "по правилу"
            var byRule = _localizationService.GetLocalizedString("ReportWindow.ByRule");
            return $"{CheckName}: {parameterValue} \"{TargetParamName}\" {CheckRule.UnfulfilledRule}" +
                $" \"{SourceParamName}\" {byRule}: {GetDictForCompareAsStr()}";
        }
    }
}
