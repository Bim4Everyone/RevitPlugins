using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitKrChecker.Models.Check {
    public class HasValueCheck : ICheck {
        public HasValueCheck(string checkName,
                             string targetParamName,
                             LevelToFind targetParamLevel) {
            CheckName = checkName ?? throw new ArgumentNullException(nameof(checkName));
            TargetParamName = targetParamName ?? throw new ArgumentNullException(nameof(targetParamName));
            TargetParamLevel = targetParamLevel;
        }

        public string CheckName { get; }
        public string TargetParamName { get; }
        public LevelToFind TargetParamLevel { get; }


        public bool Check(Element element, out CheckInfo info) {
            if(element == null)
                throw new ArgumentNullException(nameof(element));

            //var elems = GetElementsToCheck(element);

            //foreach(var elem in elems) {
            //    string targetParamValue = elem.GetParamValue<string>(TargetParamName);

            //    if(string.IsNullOrEmpty(targetParamValue)) {
            //        info = new CheckInfo(CheckName, TargetParamName, element, GetTooltip());
            //        return false;
            //    }
            //}

            var targetParams = GetParamsToCheck(element, TargetParamName, TargetParamLevel);

            foreach(var targetParam in targetParams) {
                string targetParamValue = targetParam.AsValueString();

                if(string.IsNullOrEmpty(targetParamValue)) {
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




        //private List<Element> GetElementsToCheck(Element element) {
        //    Document doc = element.Document;
        //    switch(FindLevel) {
        //        case LevelToFind.Instance:
        //            return new List<Element> { element };
        //        case LevelToFind.Type:
        //            return new List<Element> { doc.GetElement(element.GetTypeId()) };
        //        case LevelToFind.Material:
        //            return element.GetMaterialIds(false).Select(id => doc.GetElement(id)).ToList();
        //        default:
        //            throw new ArgumentOutOfRangeException(nameof(FindLevel), "Задан неизвестный уровень параметра");
        //    }
        //}

        public string GetTooltip() {
            return $"{CheckName}: значение параметра \"{TargetParamName}\" отсутствует";
        }
    }
}
