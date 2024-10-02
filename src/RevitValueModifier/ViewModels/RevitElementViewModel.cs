using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitValueModifier.Models;

namespace RevitValueModifier.ViewModels {
    internal class RevitElementViewModel : BaseViewModel {
        private readonly ILocalizationService _localizationService;
        private string _paramValue = string.Empty;
        private string _elemName;
        private ElementId _elemId;
        private Element _elem;

        public RevitElementViewModel(Element element, ILocalizationService localizationService) {
            _localizationService = localizationService;
            Elem = element;
            ElemId = element.Id;
            ElemName = Elem.Name;

            Parameters = Elem.Parameters.Cast<Parameter>().ToList();
            var elemType = Elem.GetElementType();
            Parameters.AddRange(elemType.Parameters.Cast<Parameter>().ToList());
        }

        private List<Parameter> Parameters { get; set; }

        public Element Elem {
            get => _elem;
            set => RaiseAndSetIfChanged(ref _elem, value);
        }

        public ElementId ElemId {
            get => _elemId;
            set => RaiseAndSetIfChanged(ref _elemId, value);
        }

        public string ElemName {
            get => _elemName;
            set => RaiseAndSetIfChanged(ref _elemName, value);
        }

        public string ParamValue {
            get => _paramValue;
            set => RaiseAndSetIfChanged(ref _paramValue, value);
        }

        private string GetParamValue(Parameter parameter) {
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
                var val = GetParamValue(param);
                ParamValue = regexForParam.Replace(ParamValue, val, 1);
            }
        }
    }
}
