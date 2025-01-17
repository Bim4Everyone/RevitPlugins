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
                          ParamLevel targetParamLevel,
                          ICheckRule checkRule,
                          string sourceParamName,
                          ParamLevel sourceParamLevel) {
            CheckName = checkName ?? throw new ArgumentNullException(nameof(checkName));
            TargetParamName = targetParamName ?? throw new ArgumentNullException(nameof(targetParamName));
            TargetParamLevel = targetParamLevel;
            CheckRule = checkRule ?? throw new ArgumentNullException(nameof(checkRule));
            SourceParamName = sourceParamName ?? throw new ArgumentNullException(nameof(sourceParamName));
            SourceParamLevel = sourceParamLevel;
        }

        public string CheckName { get; }
        public string TargetParamName { get; }
        public ParamLevel TargetParamLevel { get; }
        public ICheckRule CheckRule { get; }
        private string SourceParamName { get; }
        public ParamLevel SourceParamLevel { get; }


        public bool Check(Element element, out CheckInfo info) {
            Document doc = element.Document;

            if(TargetParamLevel is ParamLevel.Material && SourceParamLevel is ParamLevel.Material) {

                var materials = element.GetMaterialIds(false)
                                       .Select(id => doc.GetElement(id))
                                       .ToList();

                foreach(Element material in materials) {
                    string targetParamValue = material.GetParamValue<string>(TargetParamName);
                    string sourceParamValue = material.GetParamValue<string>(SourceParamName);

                    if(!CheckRule.Check(targetParamValue, sourceParamValue)) {
                        info = new CheckInfo(CheckName, TargetParamName, element, GetTooltip());
                        return false;
                    }
                }
                info = null;
                return true;

            } else if(TargetParamLevel != ParamLevel.Material && SourceParamLevel != ParamLevel.Material) {
                Element elementType = doc.GetElement(element.GetTypeId());
                string targetParamValue = string.Empty;
                string sourceParamValue = string.Empty;

                if(TargetParamLevel is ParamLevel.Type) {
                    targetParamValue = elementType.GetParamValue<string>(TargetParamName);
                } else {
                    targetParamValue = element.GetParamValue<string>(TargetParamName);
                }

                if(SourceParamLevel is ParamLevel.Type) {
                    sourceParamValue = elementType.GetParamValue<string>(SourceParamName);
                } else {
                    sourceParamValue = element.GetParamValue<string>(SourceParamName);
                }

                if(!CheckRule.Check(targetParamValue, sourceParamValue)) {
                    info = new CheckInfo(CheckName, TargetParamName, element, GetTooltip());
                    return false;
                }
                info = null;
                return true;

            } else {

            }


            info = null;
            return true;
        }






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


        public string GetTooltip() {
            return $"{CheckName}: значение параметра \"{TargetParamName}\" {CheckRule.UnfulfilledRule} \"{SourceParamName}\"";
        }
    }
}
