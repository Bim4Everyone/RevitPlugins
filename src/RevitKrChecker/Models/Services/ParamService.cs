using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitKrChecker.Models.Services {
    internal class ParamService {

        public Parameter GetParamToCheck(Element element, string paramName, ParamLevel paramLevel) {
            Document doc = element.Document;
            switch(paramLevel) {
                case ParamLevel.Instance:
                    return element.GetParam(paramName);
                case ParamLevel.Type:
                    return doc.GetElement(element.GetTypeId()).GetParam(paramName);
                case ParamLevel.Material:
                    return element
                        .GetMaterialIds(false)
                        .Select(id => doc.GetElement(id))
                        .Select(material => material.GetParam(paramName))
                        .FirstOrDefault();
                default:
                    throw new ArgumentOutOfRangeException(nameof(paramLevel), "Задан неизвестный уровень параметра");
            }
        }


        public List<Parameter> GetParamsToCheck(Element element, string paramName, ParamLevel paramLevel) {
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
    }
}
