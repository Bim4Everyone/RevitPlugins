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

namespace RevitValueModifier.Models {
    internal class RevitElement : INotifyPropertyChanged {
        private string _paramValue = string.Empty;
        private readonly ILocalizationService _localizationService;

        public RevitElement(Element element, ILocalizationService localizationService) {
            _localizationService = localizationService;
            Elem = element;
            ElemId = element.Id;
            var elemType = Elem.GetElementType();
            ElemName = Elem.Name;

            Parameters = Elem.Parameters.Cast<Parameter>().ToList();
            Parameters.AddRange(elemType.Parameters.Cast<Parameter>().ToList());
        }

        private List<Parameter> Parameters { get; set; }
        public Element Elem { get; }
        public ElementId ElemId { get; }
        public string ElemName { get; }

        public string ParamValue {
            get { return _paramValue; }
            set {
                _paramValue = value;
                OnPropertyChanged(nameof(ParamValue));
            }
        }

        private string GetParamValue(Parameter parameter) {
            if(!parameter.HasValue) {
                return string.Empty;
            }

            CultureInfo cultureInfo = CultureInfo.GetCultureInfo("ru-Ru");
            string value;
            switch(parameter.StorageType) {
                case StorageType.Double:
                    value = parameter.AsValueString();
                    break;
                case StorageType.ElementId:
                    ElementId id = parameter.AsElementId();

#if REVIT_2024_OR_GREATER
                    if(id.Value >= 0) {
                        value = Elem.Document.GetElement(id).Name;
                    } else {
                        value = id.Value.ToString(cultureInfo);
                    }
#else
                    if(id.IntegerValue >= 0) {
                        value = Elem.Document.GetElement(id).Name;
                    } else {
                        value = id.IntegerValue.ToString(cultureInfo);
                    }
#endif
                    break;
                case StorageType.Integer:
                    if(SpecTypeId.Boolean.YesNo == parameter.Definition.GetDataType()) {
                        if(parameter.AsInteger() == 0) {
                            value = _localizationService.GetLocalizedString("MainWindow.False");
                        } else {
                            value = _localizationService.GetLocalizedString("MainWindow.True");
                        }
                    } else {
                        value = parameter.AsInteger().ToString(cultureInfo);
                    }
                    break;
                case StorageType.String:
                    value = parameter.AsString();
                    break;
                default:
                    value = _localizationService.GetLocalizedString("MainWindow.ValueUnknown");
                    break;
            }

            return value;
        }

        public void WriteParamValue(RevitParameter revitParameter) {
            Parameter parameter = Parameters.FirstOrDefault(p => p.Id == revitParameter.Id);
            if(parameter is null) {
                TaskDialog.Show(
                    $"{_localizationService.GetLocalizedString("MainWindow.Error")}!",
                    $"{_localizationService.GetLocalizedString("MainWindow.ParamByIdError")}" + Elem.Id.ToString());
                return;
            }

            try {
                if(parameter.StorageType == StorageType.String) {
                    parameter.Set(ParamValue);
                } else if(parameter.StorageType == StorageType.Integer) {
                    var paramValueAsInt = int.Parse(ParamValue);
                    parameter.Set(paramValueAsInt);
                } else if(parameter.StorageType == StorageType.Double) {
                    var paramValueAsDouble = double.Parse(ParamValue);
                    parameter.Set(paramValueAsDouble);
                }
            } catch(System.Exception) {
                TaskDialog.Show(
                    $"{_localizationService.GetLocalizedString("MainWindow.Error")}!",
                    $"{_localizationService.GetLocalizedString("MainWindow.SettingValueError")}" + Elem.Id.ToString());
                return;
            }
        }

        public void SetParamValue(string paramValueMask) {
            // префикс_{ФОП_Блок СМР}_суффикс1_{ФОП_Секция СМР}_суффикс2
            ParamValue = paramValueMask;
            Regex regex = new Regex(@"{([^\}]+)}");
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

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "") {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
