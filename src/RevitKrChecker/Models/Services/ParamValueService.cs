using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitKrChecker.Models.Services {
    internal class ParamValueService {
        public string GetParamValueToCheck(Element element, string paramName, ParamLevel paramLevel) {
            Document doc = element.Document;
            switch(paramLevel) {
                case ParamLevel.Instance:
                    return element.GetParam(paramName).AsValueString();
                case ParamLevel.Type:
                    return doc.GetElement(element.GetTypeId()).GetParam(paramName).AsValueString();
                case ParamLevel.Material:
                    return element
                        .GetMaterialIds(false)
                        .Select(id => doc.GetElement(id))
                        .Select(material => material.GetParam(paramName).AsValueString())
                        .FirstOrDefault();
                default:
                    throw new ArgumentOutOfRangeException(nameof(paramLevel), "Задан неизвестный уровень параметра");
            }
        }

        public List<string> GetParamValuesToCheck(Element element, string paramName, ParamLevel paramLevel) {
            Document doc = element.Document;
            switch(paramLevel) {
                case ParamLevel.Instance:
                    return new List<string> {
                        element.GetParam(paramName).AsValueString()
                    };
                case ParamLevel.Type:
                    return new List<string> {
                        doc.GetElement(element.GetTypeId()).GetParam(paramName).AsValueString()
                    };
                case ParamLevel.Material:
                    return element
                        .GetMaterialIds(false)
                        .Select(id => doc.GetElement(id))
                        .Select(material => material.GetParam(paramName).AsValueString())
                        .ToList();
                default:
                    throw new ArgumentOutOfRangeException(nameof(paramLevel), "Задан неизвестный уровень параметра");
            }
        }




        //private bool CheckAllValues(string targetParamValue, List<string> sourceParamValues) {
        //    return sourceParamValues.All(sourceParamValue => CheckRule.Check(targetParamValue, sourceParamValue));
        //}


        //private bool CheckAllValues(List<string> targetParamValues, string sourceParamValue) {
        //    return targetParamValues.All(targetParamValue => CheckRule.Check(targetParamValue, sourceParamValue));
        //}

        //private bool CheckAllValues(List<string> targetParamValues, List<string> sourceParamValues) {
        //    return targetParamValues
        //        .All(targetParamValue => sourceParamValues
        //            .All(sourceParamValue => CheckRule.Check(targetParamValue, sourceParamValue)));
        //}
    }
}
