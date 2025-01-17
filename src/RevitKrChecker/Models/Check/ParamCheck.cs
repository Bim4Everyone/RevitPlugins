using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitKrChecker.Models.Rule;

namespace RevitKrChecker.Models.Check {
    public class ParamCheck : ICheck {

        public ParamCheck(string checkName,
                          string targetParamName,
                          ParamLevel targetParamLevel,
                          ICheckRule checkRule,
                          List<string> trueValues) {
            CheckName = checkName ?? throw new ArgumentNullException(nameof(checkName));
            TargetParamName = targetParamName ?? throw new ArgumentNullException(nameof(targetParamName));
            TargetParamLevel = targetParamLevel;
            CheckRule = checkRule ?? throw new ArgumentNullException(nameof(checkRule));
            TrueValues = trueValues ?? throw new ArgumentNullException(nameof(trueValues));
        }

        public string CheckName { get; }
        public string TargetParamName { get; }
        public ParamLevel TargetParamLevel { get; }
        public ICheckRule CheckRule { get; }
        public List<string> TrueValues { get; }


        private List<Parameter> GetParamsToCheck(Element element, string paramName, ParamLevel paramLevel) {
            Document doc = element.Document;
            switch(paramLevel) {
                case ParamLevel.Instance:
                    return new List<Parameter> {
                        element.GetParam(paramName)
                    };
                case ParamLevel.Type:
                    return new List<Parameter> {
                        doc.GetElement(element.GetTypeId()).GetParam(paramName)
                    };
                case ParamLevel.Material:
                    return element
                        .GetMaterialIds(false)
                        .Select(id => doc.GetElement(id))
                        .Select(material => material.GetParam(paramName))
                        .ToList();
                default:
                    throw new ArgumentOutOfRangeException(nameof(paramLevel), "Задан неизвестный уровень параметра");
            }
        }

        private bool CheckAnyTrueValues(string targetParamValue) {
            return TrueValues.Any(trueValue => CheckRule.Check(targetParamValue, trueValue));
        }

        public bool Check(Element element, out CheckInfo info) {
            if(element == null)
                throw new ArgumentNullException(nameof(element));

            var targetParams = GetParamsToCheck(element, TargetParamName, TargetParamLevel);

            foreach(var targetParam in targetParams) {
                string targetParamValue = targetParam.AsValueString();

                if(!CheckAnyTrueValues(targetParamValue)) {
                    info = new CheckInfo(CheckName, TargetParamName, element, GetTooltip());
                    return false;
                }
            }

            info = null;
            return true;
        }

        private string GetTrueValuesAsStr() {
            string answer = string.Empty;
            string separator = ", ";

            foreach(string trueValue in TrueValues) {
                answer += $"{trueValue}{separator}";
            }
            answer = answer.Substring(0, answer.LastIndexOf(separator));
            return answer;
        }

        public string GetTooltip() {
            return $"{CheckName}: значение параметра \"{TargetParamName}\" {CheckRule.UnfulfilledRule} {GetTrueValuesAsStr()}";
        }
    }
}
