using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitKrChecker.Models.CheckOptions;
using RevitKrChecker.Models.Interfaces;
using RevitKrChecker.Models.Services;

namespace RevitKrChecker.Models.Check {
    public class TemplatesCompareElemParamsCheck : ICheck {
        private readonly ParamService _paramService;

        /// <summary>
        /// Проверка для анализа параметра на уровне экземпляра или типоразмера элемента.
        /// Для использования необходимо указать в <see cref="ParamCheckOptions"/>:
        /// имя проверки, имя параметра для проверки, уровень параметра для проверки, правило проверки, 
        /// имя параметра для сопоставления, уровень параметра для сопоставления и словарь для сопоставления
        /// </summary>
        public TemplatesCompareElemParamsCheck(TemplatesCompareCheckOptions checkOptions) {
            CheckName = checkOptions.CheckName
                ?? throw new ArgumentNullException(nameof(checkOptions.CheckName));
            TargetParamName = checkOptions.TargetParamName
                ?? throw new ArgumentNullException(nameof(checkOptions.TargetParamName));
            // Проверяем, что параметр для проверке не на уровне материала
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

            DictForCompare = checkOptions.DictForCompare
                ?? throw new ArgumentNullException(nameof(checkOptions.DictForCompare));

            _paramService = new ParamService();
        }

        public string CheckName { get; }
        public string TargetParamName { get; }
        public ParamLevel TargetParamLevel { get; }
        public ICheckRule CheckRule { get; }
        private string SourceParamName { get; }
        public ParamLevel SourceParamLevel { get; }
        private Dictionary<string, string> DictForCompare { get; }


        public bool Check(Element element, out CheckInfo info) {
            Parameter targetParam = _paramService.GetParamToCheck(element, TargetParamName, TargetParamLevel);
            Parameter sourceParam = _paramService.GetParamToCheck(element, SourceParamName, SourceParamLevel);

            string targetParamValue = targetParam.AsValueString();
            string sourceParamValue = sourceParam.AsValueString();
            string dictSourceParamValue = DictForCompare[sourceParamValue];

            if(CheckRule.Check(targetParamValue, dictSourceParamValue)) {
                info = null;
                return true;
            }
            info = new CheckInfo(CheckName, TargetParamName, element, GetTooltip());
            return false;
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
