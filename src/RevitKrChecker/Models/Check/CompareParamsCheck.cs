using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitKrChecker.Models.Rule;

namespace RevitKrChecker.Models.Check {
    public class CompareParamsCheck : ICheck {
        public CompareParamsCheck(string checkName,
                          string targetParamName,
                          LevelToFind targetParamLevel,
                          ICheckRule checkRule,
                          string sourceParamName,
                          LevelToFind sourceParamLevel) {
            CheckName = checkName ?? throw new ArgumentNullException(nameof(checkName));
            TargetParamName = targetParamName ?? throw new ArgumentNullException(nameof(targetParamName));
            TargetParamLevel = targetParamLevel;
            CheckRule = checkRule ?? throw new ArgumentNullException(nameof(checkRule));
            SourceParamName = sourceParamName ?? throw new ArgumentNullException(nameof(sourceParamName));
            SourceParamLevel = sourceParamLevel;
        }

        public string CheckName { get; }
        public string TargetParamName { get; }
        public LevelToFind TargetParamLevel { get; }
        public ICheckRule CheckRule { get; }
        private string SourceParamName { get; }
        public LevelToFind SourceParamLevel { get; }

        public bool Check(Element element, out CheckInfo info) {
            //string targetParamValue = element.GetParamValue<string>(TargetParamName);
            //string sourceParamValue = element.GetParamValue<string>(SourceParamName);

            //if(CheckRule.Check(targetParamValue, sourceParamValue)) {
            //    info = null;
            //    return true;
            //}

            //info = new CheckInfo(CheckName, TargetParamName, element, GetTooltip());
            //return false;



            var targetParams = GetParamsToCheck(element, TargetParamName, TargetParamLevel);

            foreach(var targetParam in targetParams) {
                string targetParamValue = targetParam.AsValueString();

                if(!CheckTrueValues(targetParamValue)) {
                    info = new CheckInfo(CheckName, TargetParamName, element, GetTooltip());
                    return false;
                }
            }

            info = null;
            return true;
        }


        private List<Parameter> GetParamsToCheck(Element element, string paramName, LevelToFind paramLevel) {
            Document doc = element.Document;
            switch(paramLevel) {
                case LevelToFind.Instance:
                    return new List<Parameter> {
                        element.GetParam(paramName)
                    };
                case LevelToFind.Type:
                    return new List<Parameter> {
                        doc.GetElement(element.GetTypeId()).GetParam(paramName)
                    };
                case LevelToFind.Material:
                    return element
                        .GetMaterialIds(false)
                        .Select(id => doc.GetElement(id))
                        .Select(material => material.GetParam(paramName))
                        .ToList();
                default:
                    throw new ArgumentOutOfRangeException(nameof(paramLevel), "Задан неизвестный уровень параметра");
            }
        }


        public string GetTooltip() {
            return $"{CheckName}: значение параметра \"{TargetParamName}\" {CheckRule.UnfulfilledRule} \"{SourceParamName}\"";
        }
    }
}
