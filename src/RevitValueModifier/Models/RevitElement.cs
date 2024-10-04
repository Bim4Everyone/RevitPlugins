using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitValueModifier.Models {
    internal class RevitElement {
        private readonly ILocalizationService _localizationService;

        public RevitElement(Element element, ILocalizationService localizationService) {
            _localizationService = localizationService;
            Elem = element;
            ElemId = element.Id;
            ElemName = Elem.Name;

            Parameters = Elem.Parameters.Cast<Parameter>().ToList();
            var elemType = Elem.GetElementType();
            Parameters.AddRange(elemType.Parameters.Cast<Parameter>().ToList());
        }

        private List<Parameter> Parameters { get; set; }

        public Element Elem { get; set; }

        public ElementId ElemId { get; set; }

        public string ElemName { get; set; }

        public string ParamValue { get; set; } = string.Empty;

        private string GetValueByParameter(Parameter parameter) {
            if(!parameter.HasValue) {
                return string.Empty;
            }
            string value = parameter.AsValueString();
            if(string.IsNullOrEmpty(value)) {
                return parameter.AsObject().ToString();
            }
            return value;
        }

        public void WriteParamValue(RevitParameter revitParameter) {
            Parameter parameter = Parameters.FirstOrDefault(p => p.Id == revitParameter.Id);
            if(parameter is null) {
                throw new Exception($"{_localizationService.GetLocalizedString("MainWindow.ParamByIdError")}" + Elem.Id.ToString());
            }
            try {
                parameter.Set(ParamValue);
            } catch(Exception) {
                throw new Exception($"{_localizationService.GetLocalizedString("MainWindow.SettingValueError")}" + Elem.Id.ToString());
            }
        }

        public void SetParamValue(string paramValueMask) {
            // префикс_{ФОП_Блок СМР}_суффикс1_{ФОП_Секция СМР}_суффикс2
            ParamValue = paramValueMask;
            var regex = new Regex(@"{([^\}]+)}");
            MatchCollection matches = regex.Matches(paramValueMask);

            Regex regexForParam;
            foreach(Match match in matches) {
                string paramName = match.Value.Replace("{", "").Replace("}", "");
                Parameter param = Parameters
                    .FirstOrDefault(parameter => parameter.Definition.Name == paramName);
                if(param == null) { return; }

                regexForParam = new Regex(match.Value);
                var val = GetValueByParameter(param);
                ParamValue = regexForParam.Replace(ParamValue, val, 1);
            }
        }
    }
}
