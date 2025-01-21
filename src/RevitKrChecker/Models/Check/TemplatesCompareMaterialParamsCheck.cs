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
        private readonly ParamService _paramService;

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
            SourceParamLevel = checkOptions.SourceParamLevel;

            DictForCompare = checkOptions.DictForCompare
                ?? throw new ArgumentNullException(nameof(checkOptions.DictForCompare));

            _paramService = new ParamService();
        }

        public string CheckName { get; }
        public string TargetParamName { get; }
        public ICheckRule CheckRule { get; }
        private string SourceParamName { get; }
        public ParamLevel SourceParamLevel { get; }
        public Dictionary<string, string> DictForCompare { get; }


        public bool Check(Element element, out CheckInfo info) {
            Document doc = element.Document;

            // Если сравнивать будем не с параметром материала, а с параметров экземпляра или типа,
            // то значение будет одно и то же для разных материалов. Получать его потом много раз в цикле нет смысла
            // В этом случае получим значение, с которым будем сравнивать сразу
            string dictSourceParamValue = string.Empty;
            if(SourceParamLevel != ParamLevel.Material) {
                Parameter sourceParam = _paramService.GetParamToCheck(element, SourceParamName, SourceParamLevel);
                string sourceParamValue = sourceParam.AsValueString();
                dictSourceParamValue = DictForCompare[sourceParamValue];
            }

            List<Element> materials = element.GetMaterialIds(false)
                           .Select(id => doc.GetElement(id))
                           .ToList();

            foreach(Element material in materials) {
                string targetParamValue = material.GetParam(TargetParamName).AsValueString();

                // Если сравнивать будем со значением параметра на уровне материала,
                // то будем получать его значение каждый раз в рамках материала
                if(SourceParamLevel is ParamLevel.Material) {
                    string sourceParamValue = material.GetParam(SourceParamName).AsValueString();
                    dictSourceParamValue = DictForCompare[sourceParamValue];
                }

                if(!CheckRule.Check(targetParamValue, dictSourceParamValue)) {
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
