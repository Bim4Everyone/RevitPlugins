using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;

namespace RevitValueModifier.Models {
    internal class RevitElement : INotifyPropertyChanged {
        private string _paramValue = string.Empty;

        public RevitElement(Element element) {
            Elem = element;
            var elemType = Elem.GetElementType();

            Parameters = Elem.Parameters.Cast<Parameter>().ToList();
            Parameters.AddRange(elemType.Parameters.Cast<Parameter>().ToList());
        }

        public Element Elem { get; }
        public List<Parameter> Parameters { get; set; }

        public string ParamValue {
            get { return _paramValue; }
            set {
                _paramValue = value;
                OnPropertyChanged("ParamValue");
            }
        }

        public void WriteParamValue(RevitParameter revitParameter) {
            Parameter parameter = Parameters.FirstOrDefault(p => p.Id == revitParameter.Id);
            if(parameter is null) {
                TaskDialog.Show("Ошибка!", "Не найден выбранный для записи параметр в элементе с id "
                    + Elem.Id.ToString());
            }

            if(parameter.StorageType == StorageType.String) {
                parameter.Set(ParamValue);
            } else if(parameter.StorageType == StorageType.Integer) {
                var paramValueAsInt = int.Parse(ParamValue);
                parameter.Set(paramValueAsInt);
            } else if(parameter.StorageType == StorageType.Double) {
                var paramValueAsDouble = double.Parse(ParamValue);
                parameter.Set(paramValueAsDouble);
            }
        }

        public void SetParamValue(string paramValueMask) {
            // заранее реализовать проверку, что символы { } парны
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

        public string GetParamValue(Parameter parameter) {
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
                            value = "False";
                        } else {
                            value = "True";
                        }
                    } else {
                        value = parameter.AsInteger().ToString(cultureInfo);
                    }
                    break;
                case StorageType.String:
                    value = parameter.AsString();
                    break;
                default:
                    value = "Значение неизвестно";
                    break;
            }

            return value;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "") {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
